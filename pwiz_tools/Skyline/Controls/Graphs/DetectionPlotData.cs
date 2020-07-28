﻿using pwiz.Skyline.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using pwiz.Common.Collections;
using pwiz.Common.SystemUtil;
using pwiz.Skyline.Util;
// ReSharper disable InconsistentlySynchronizedField

namespace pwiz.Skyline.Controls.Graphs
{
    public class DetectionPlotData : IDisposable
    {
        public static DetectionDataCache DataCache  = new DetectionDataCache();

        private List<QData> _precursorData;
        private List<QData> _peptideData;
        private bool _isValid;
        private Dictionary<DetectionsGraphController.TargetType, DataSet> _data = new Dictionary<DetectionsGraphController.TargetType, DataSet>();

        public SrmDocument Document { get; private set; }
        public float QValueCutoff { get; private set; }
        public bool IsValid => _isValid;
        public int ReplicateCount { get; private set; }

        public DataSet GetTargetData(DetectionsGraphController.TargetType target)
        {
            return _data[target];

        }

        public List<string> ReplicateNames { get; private set; }

        public static DetectionPlotData INVALID = new DetectionPlotData(null, 0.001f);
        public const int REPORTING_STEP = 3;

        public DetectionPlotData(SrmDocument document, float qValueCutoff, 
            CancellationToken? cancellationToken = null, [CanBeNull] Action<int> progressReport = null)
        {
            if (document == null || qValueCutoff == 0 || qValueCutoff == 1 ||
                !document.Settings.HasResults) return;

            if (document.MoleculeTransitionGroupCount == 0 || document.PeptideCount == 0 ||
                document.MeasuredResults.Chromatograms.Count == 0)
                return;

            QValueCutoff = qValueCutoff;
            Document = document;

            _precursorData = new List<QData>(document.MoleculeTransitionGroupCount);
            _peptideData = new List<QData>(document.PeptideCount);
            ReplicateCount = document.MeasuredResults.Chromatograms.Count;


            ReplicateNames = (from chromatogram in document.MeasuredResults.Chromatograms
                select chromatogram.Name).ToList();

            var qs = new List<float>(ReplicateCount);
            var _thisPeptideData = new List<List<float>>();
            int peptideCount = 0;
            int currentProgress = 0;
            var reportingStep = document.PeptideCount / (90/REPORTING_STEP);

            foreach (var peptide in document.Peptides)
            {
                _thisPeptideData.Clear();
                //iterate over peptide's precursors
                foreach (var precursor in peptide.TransitionGroups)
                {
                    if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                        return;

                    if (precursor.IsDecoy) continue;
                    qs.Clear();
                    //get q-values for precursor replicates
                    foreach (var i in Enumerable.Range(0, ReplicateCount).ToArray())
                    {
                        var chromInfo = precursor.GetSafeChromInfo(i).FirstOrDefault(c => c.OptimizationStep == 0);
                        if (chromInfo != null && chromInfo.QValue.HasValue)
                            qs.Add(chromInfo.QValue.Value);
                        else
                            qs.Add(float.NaN);
                    }

                    _precursorData.Add(new QData(precursor.Id, qs));
                    _thisPeptideData.Add(qs.ToList());
                }

                if (_thisPeptideData.Count > 0)
                {
                    _peptideData.Add(new QData(peptide.Id, 
                        Enumerable.Range(0, ReplicateCount).Select(
                            i =>
                            {
                                var min = new Statistics(_thisPeptideData.Select(lst => (double) lst[i])).Min();
                                return (float) min;
                            }).ToList()
                    ));
                }

                if(peptideCount++ == reportingStep * currentProgress)
                    progressReport?.Invoke(REPORTING_STEP * currentProgress++);
            }

            _data[DetectionsGraphController.TargetType.PRECURSOR] = new DataSet(_precursorData, ReplicateCount, QValueCutoff);
            _data[DetectionsGraphController.TargetType.PEPTIDE] = new DataSet(_peptideData, ReplicateCount, QValueCutoff);

            _isValid = true;
        }

        private bool IsValidFor(SrmDocument document, float qValue)
        {
            return document != null && Document != null && _isValid &&
                   ReferenceEquals(document, Document) &&
                   qValue == QValueCutoff;
        }

        public void Dispose()
        {
            _precursorData.Clear();
            _peptideData.Clear();
        }
        public class DataSet
        {
            public ImmutableList<int> TargetsCount { get; private set; }
            public ImmutableList<int> TargetsCumulative { get; private set; }
            public ImmutableList<int> TargetsAll { get; private set; }
            public ImmutableList<float> QMedians { get; private set; }
            public ImmutableList<int> Histogram { get; private set; }


            public double MaxCount
            {
                get { return new Statistics(TargetsCumulative.Select(i => (double)i)).Max(); }
            }

            public DataSet(List<QData> data, int replicateCount, float qValueCutoff,
                CancellationToken? cancellationToken = null, [CanBeNull] Action<int> progressReport = null)
            {
                TargetsCount = ImmutableList<int>.ValueOf(Enumerable.Range(0, replicateCount)
                    .Select(i => data.Count(t => t.QValues[i] < qValueCutoff)));
                CancelOrReport(92, cancellationToken, progressReport);

                QMedians = ImmutableList<float>.ValueOf(Enumerable.Range(0, replicateCount)
                    .Select(i =>
                    {
                        var qStats = new Statistics(
                            data.FindAll(t => t.QValues[i] < qValueCutoff)
                                .Select(t => (double)t.QValues[i]));
                        return (float)qStats.Median();
                    }));
                CancelOrReport(94, cancellationToken, progressReport);

                TargetsCumulative = ImmutableList<int>.ValueOf(Enumerable.Range(0, replicateCount)
                    .Select(i => data.Count(t => t.MinQValues[i] < qValueCutoff)));
                CancelOrReport(96, cancellationToken, progressReport);
                TargetsAll = ImmutableList<int>.ValueOf(Enumerable.Range(0, replicateCount)
                    .Select(i => data.Count(t => t.MaxQValues[i] < qValueCutoff)));
                CancelOrReport(98, cancellationToken, progressReport);

                var histogramPairs = data.Select(t => t.QValues.Count(f => f < qValueCutoff)) //Count replicates for each target
                    .GroupBy(f => f, c => 1,
                        (f, c) => new
                            {replicateNum = f, histCount = c.Sum()}).ToLookup((tuple)=> tuple.replicateNum); //Group targets by the number of replicates

                Histogram = ImmutableList<int>.ValueOf(Enumerable.Range(1, replicateCount + 1)
                    .Select(n => histogramPairs.Contains(n) ? histogramPairs[n].First().histCount : 0));
                CancelOrReport(100, cancellationToken, progressReport);
            }

            private void CancelOrReport(int percent,
                CancellationToken? cancellationToken = null, [CanBeNull] Action<int> progressReport = null)
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    throw new OperationCanceledException();
                
                progressReport?.Invoke(percent);
            }
            /// <summary>
            /// Returns count of targets detected in at least minRep replicates
            /// </summary>
            public int getCountForMinReplicates(int minRep)
            {
                if (minRep > Histogram.Count) return 0;
                return Enumerable.Range(Math.Max(minRep-1, 0), Histogram.Count - Math.Max(minRep - 1, 0)).Select(i => Histogram[i]).Sum();
            }
        }

        /// <summary>
        /// List of q-values across replicates for a single target (peptide or precursor)
        /// It is also equipped with lists of running mins and maxes for this target.
        /// </summary>
        public class QData
        {
            public QData(Identity target, List<float> qValues)
            {
                Target = target;
                QValues = ImmutableList.ValueOf(qValues);

                // Calculate running mins and maxes while taking NaNs into account
                var mins = Enumerable.Repeat(float.NaN, qValues.Count).ToList();
                var maxes = Enumerable.Repeat(float.NaN, qValues.Count).ToList();
                if (!qValues.All(float.IsNaN))
                {
                    bool runningNaN = true;
                    for (int i = 0; i < qValues.Count; i++)
                    {
                        //if this and all previous values are NaN
                        if (float.IsNaN(qValues[i]))
                        {
                            if (!runningNaN)
                            {
                                mins[i] = mins[i - 1];
                                maxes[i] = maxes[i - 1];
                            }
                        }
                        else
                        {
                            if (runningNaN)
                            {
                                mins[i] = maxes[i] = qValues[i];
                                runningNaN = false;
                            }
                            else
                            {
                                mins[i] = (mins[i - 1] > qValues[i]) ? qValues[i] : mins[i - 1];
                                maxes[i] = (maxes[i - 1] < qValues[i]) ? qValues[i] : maxes[i - 1];
                            }
                        }
                    }
                }
                MinQValues = ImmutableList.ValueOf(mins);
                MaxQValues = ImmutableList.ValueOf(maxes);
            }
            public Identity Target { get; private set; }

            public ImmutableList<float> QValues { get; private set; }
            public ImmutableList<float> MinQValues { get; private set; }
            public ImmutableList<float> MaxQValues { get; private set; }

        }

        public class DetectionDataCache
        {
            private class DataRequest
            {
                public float qValue;
            }

            public SrmDocument _document;
            private ConcurrentQueue<DetectionPlotData> _datas;
            private StackWorker<DataRequest> _stackWorker;
            private CancellationTokenSource _tokenSource;
            private Action<DetectionPlotData> _callback;
            private object _statusLock = new object();

            // This is max number of replicates that the cache will store before starting to purge 
            // the old datasets. Number of replicates is a good proxy for the amount of memory used 
            // by a dataset.
            private const int CACHE_CAPACITY = 200;

            public event Action<CacheStatus> StatusChange;
            public event Action<int> ReportProgress;

            public enum CacheStatus { idle, processing, error, canceled }

            private CacheStatus _status;
            public CacheStatus Status
            {
                get => _status;
                set
                {
                    _status = value;
                    StatusChange?.Invoke(_status);
                }
            }

            public DetectionDataCache()
            {
                _stackWorker = new StackWorker<DataRequest>(null, CacheData);
                //single worker thread because we do not need to paralellize the calculations, 
                //only to offload them from the UI thread
                _stackWorker.RunAsync(1, @"DetectionsDataCache");
                _tokenSource = new CancellationTokenSource();
                _datas = new ConcurrentQueue<DetectionPlotData>();
                Status = CacheStatus.idle;
            }

            public bool TryGet(SrmDocument doc, float qValue, Action<DetectionPlotData> callback,  out DetectionPlotData data)
            {
                //Debug.WriteLine($@"Received request for q {qValue}");
                data = DetectionPlotData.INVALID;
                var request = new DataRequest() { qValue = qValue};
                if (ReferenceEquals(doc, _document))
                {
                    data = Get(request) ?? DetectionPlotData.INVALID;
                    if (data.IsValid)
                    {
                        //Debug.WriteLine($@"Found cached data for q {qValue}");
                        return true;
                    }
                    //Debug.WriteLine($@"No cached data for q {qValue}, adding to the queue");
                    _callback = callback;
                    _stackWorker.Add(request);
                }
                else
                {
                    _document = doc;
                    //Debug.WriteLine($@"New document request for q {qValue}, starting cancel.");
                    new Task(CancelWorker, new object[] { request, false }).Start(); 
                }

                return false;
            }

            public void Cancel()
            {
                new Task(CancelWorker, new object[]{null, true}).Start();

            }

            private void CancelWorker(object param)
            {
                bool userCancel = (bool)((object[]) param)[1];
                //signal cancel to other workers and wait
                _tokenSource.Cancel();
                //Debug.WriteLine($@"Cancel requested.");
                var oldTokenSource = _tokenSource;
                _tokenSource = new CancellationTokenSource();
                lock (_statusLock)  //Wait for the current worker to complete
                {
                    //Debug.WriteLine($@"Cancel complete.");
                    oldTokenSource.Dispose();
                    //purge the queue
                    var queueLength = _datas.Count;
                    for (var i = 0; i < queueLength; i++)
                    {
                        if(_datas.TryDequeue(out var dump))
                            if(ReferenceEquals(_document, dump.Document))
                                _datas.Enqueue(dump);
                    }
                    if(userCancel)
                        Status = CacheStatus.canceled;
                    else
                        Status = CacheStatus.idle;
                }
                //if provided, add the new request to the queue after cancellation is complete
                if (((object[])param)[0] is DataRequest req)
                {
                    //Debug.WriteLine($@"Adding new document request for q {req.qValue}");
                    _stackWorker.Add(req);
                }

            }

            private DetectionPlotData Get(DataRequest request)
            {
                return _datas.FirstOrDefault(d => d.IsValidFor(_document, request.qValue));
            }

            //Worker thread method
            private void CacheData(DataRequest request, int index)
            {
                try
                {
                    lock (_statusLock)
                    {
                        //Debug.WriteLine($@"Worker: Starting thread for request for q {request.qValue}");
                        //first make sure it hasn't been retrieved already
                        //and calculate the queue size
                        int currentSize = 0;
                        DetectionPlotData res = null;
                        foreach (var dat in _datas)
                        {
                            currentSize += dat.ReplicateCount;
                            if (dat.IsValidFor(_document, request.qValue)) res = dat;
                        }

                        if (res != null)
                        {
                            //Debug.WriteLine($@"Worker: Found cached data for request for q {request.qValue}");
                            return;
                        }
                        //Debug.WriteLine($@"Worker: No cached data for request for q {request.qValue}, retrieving data.");
                        Status = CacheStatus.processing;
                        res = new DetectionPlotData(_document, request.qValue, _tokenSource.Token, ReportProgress);
                        Status = CacheStatus.idle;

                        if (res.IsValid)
                        {
                            //Debug.WriteLine($@"Worker: Data retrieval successful for request for q {request.qValue}.");
                            if (currentSize + res.ReplicateCount >= CACHE_CAPACITY) _datas.TryDequeue(out var dump);
                            _datas.Enqueue(res);
                            _callback.Invoke(res);
                        }
                        else
                        {
                            //Debug.WriteLine($@"Worker: Data retrieval failed for request for q {request.qValue}.");
                        }
                    }
                }
                catch (Exception)
                {
                    //Debug.WriteLine($@"Worker: Exception for request for q {request.qValue}.");
                    Status = CacheStatus.error;
                    throw;
                }
            }
        }
    }

}
