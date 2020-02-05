using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Skyline.Alerts;
using pwiz.Skyline.Controls.GroupComparison;
using pwiz.Skyline.EditUI;
using pwiz.SkylineTestUtil;

namespace pwiz.SkylineTestFunctional
{
    [TestClass]
    public class GroupComparisonRefineTest : AbstractFunctionalTestEx
    {

        [TestMethod]
        public void TestGroupComparisonRefine()
        {
            TestFilesZip = "TestFunctional/AreaCVHistogramTest.zip";
            RunFunctionalTest();
        }

        protected override void DoTest()
        {
            OpenDocument(@"Rat_plasma.sky");

            // Create new group comparison
            CreateGroupComparison("Test Group Comparison", "Condition", "Healthy", "Diseased");

            // Use volcano plot to generate values to compare against
            var grid = ShowDialog<FoldChangeGrid>(() => SkylineWindow.ShowGroupComparisonWindow("Test Group Comparison"));

            // Wait for grid and show volcano plot
            var volcanoPlot = ShowDialog<FoldChangeVolcanoPlot>(() => grid.ShowVolcanoPlot());
            volcanoPlot.UseOverridenKeys = true;

            // Set bounds to p value < 0.05 and fold-change 2
            GroupComparisonVolcanoPlotTest.OpenVolcanoPlotProperties(volcanoPlot, p =>
                {
                    p.TextFoldChangeCutoff.Text = (1.0).ToString(CultureInfo.CurrentCulture);
                    p.TextPValueCutoff.Text = (-Math.Log10(0.05)).ToString(CultureInfo.CurrentCulture);
                });
            var document = SkylineWindow.Document;
            RunUI(volcanoPlot.RemoveBelowCutoffs);
            WaitForDocumentChange(document);
            var plotStateCutoff = (SkylineWindow.Document.PeptideGroupCount, SkylineWindow.Document.PeptideCount, SkylineWindow.Document.PeptideTransitionGroupCount,
                SkylineWindow.Document.PeptideTransitionCount);
            RunUI(SkylineWindow.Undo);
            //WaitForDocumentChange(document);
            
            WaitForCondition(() => ReferenceEquals(volcanoPlot.FoldChangeBindingSource.GroupComparisonModel.Results?.Document, SkylineWindow.Document));
            WaitForConditionUI(() => grid.DataboundGridControl.IsComplete);

            GroupComparisonVolcanoPlotTest.OpenVolcanoPlotProperties(volcanoPlot, p =>
                {
                    p.CheckBoxLog.Checked = false;
                    p.TextFoldChangeCutoff.Text = (3.0).ToString(CultureInfo.CurrentCulture);
                    p.TextPValueCutoff.Text = "";
                });
            document = SkylineWindow.Document;
            RunUI(volcanoPlot.RemoveBelowCutoffs);
            WaitForDocumentChange(document);
            var plotStateFC = (SkylineWindow.Document.PeptideGroupCount, SkylineWindow.Document.PeptideCount, SkylineWindow.Document.PeptideTransitionGroupCount,
                SkylineWindow.Document.PeptideTransitionCount);
            RunUI(SkylineWindow.Undo);
            WaitForCondition(()=>ReferenceEquals(volcanoPlot.FoldChangeBindingSource.GroupComparisonModel.Results?.Document, SkylineWindow.Document));

            GroupComparisonVolcanoPlotTest.OpenVolcanoPlotProperties(volcanoPlot, p =>
            {
                p.CheckBoxLog.Checked = false;
                p.TextFoldChangeCutoff.Text = "";
                p.TextPValueCutoff.Text = (0.08).ToString(CultureInfo.CurrentCulture);
            });
            document = SkylineWindow.Document;
            RunUI(volcanoPlot.RemoveBelowCutoffs);
            WaitForDocumentChange(document);
            var plotStatePval = (SkylineWindow.Document.PeptideGroupCount, SkylineWindow.Document.PeptideCount, SkylineWindow.Document.PeptideTransitionGroupCount,
                SkylineWindow.Document.PeptideTransitionCount);
            RunUI(SkylineWindow.Undo);
            WaitForCondition(() => ReferenceEquals(volcanoPlot.FoldChangeBindingSource.GroupComparisonModel.Results?.Document, SkylineWindow.Document));
            
            var graphStates = new[] { plotStateCutoff, plotStateFC, plotStatePval, (48, 44, 44, 255) };

            // Verify that bad inputs show error message
            var refineDlg = ShowDialog<RefineDlg>(() => SkylineWindow.ShowRefineDlg());
            RunUI(() =>
            {
                refineDlg.Log = false;
                refineDlg.FoldChangeCutoff = -1;
            });

            var alertDlg = ShowDialog<AlertDlg>(refineDlg.OkDialog);
            OkDialog(alertDlg, alertDlg.OkDialog);

            RunUI(() =>
            {
                refineDlg.FoldChangeCutoff = 2;
                refineDlg.AdjustedPValueCutoff = 0;
            });
            alertDlg = ShowDialog<AlertDlg>(refineDlg.OkDialog);
            OkDialog(alertDlg, alertDlg.OkDialog);

            // Verify remove below cutoff works
            RunUI(() =>
            {
                refineDlg.Log = false;
                refineDlg.AdjustedPValueCutoff = 0.05;
                refineDlg.FoldChangeCutoff = 2;
            });
            var docChange = SkylineWindow.Document;
            OkDialog(refineDlg, refineDlg.OkDialog);
            WaitForDocumentChange(docChange);
            var doc = SkylineWindow.Document;
            var refineDocState = (doc.PeptideGroupCount, doc.PeptideCount, doc.PeptideTransitionGroupCount,
                doc.PeptideTransitionCount);
            Assert.AreEqual(graphStates[0], refineDocState);
            RunUI(SkylineWindow.Undo);

            // Verify that using only fold change cutoff works
            refineDlg = ShowDialog<RefineDlg>(() => SkylineWindow.ShowRefineDlg());
            RunUI(() =>
            {
                refineDlg.Log = false;
                refineDlg.FoldChangeCutoff = 3;
            });
            docChange = SkylineWindow.Document;
            OkDialog(refineDlg, refineDlg.OkDialog);
            WaitForDocumentChange(docChange);
            doc = SkylineWindow.Document;
            refineDocState = (doc.PeptideGroupCount, doc.PeptideCount, doc.PeptideTransitionGroupCount,
                doc.PeptideTransitionCount);
            Assert.AreEqual(graphStates[1], refineDocState);
            RunUI(SkylineWindow.Undo);

            // Verify using only adjusted p value cutoff works
            refineDlg = ShowDialog<RefineDlg>(() => SkylineWindow.ShowRefineDlg());
            RunUI(() =>
            {
                refineDlg.Log = false;
                refineDlg.AdjustedPValueCutoff = 0.08;
            });
            docChange = SkylineWindow.Document;
            OkDialog(refineDlg, refineDlg.OkDialog);
            WaitForDocumentChange(docChange);
            doc = SkylineWindow.Document;
            refineDocState = (doc.PeptideGroupCount, doc.PeptideCount, doc.PeptideTransitionGroupCount,
                doc.PeptideTransitionCount);
            Assert.AreEqual(graphStates[2], refineDocState);
            RunUI(SkylineWindow.Undo);

            // Verify the union of 2 group comparisons works
            CreateGroupComparison("Test Group Comparison 2", "Condition", "Healthy", "Diseased", "BioReplicate");

            refineDlg = ShowDialog<RefineDlg>(() => SkylineWindow.ShowRefineDlg());
            RunUI(() =>
            {
                refineDlg.Log = false;
                refineDlg.AdjustedPValueCutoff = 0.05;
                refineDlg.FoldChangeCutoff = 2;
            });
            docChange = SkylineWindow.Document;
            OkDialog(refineDlg, refineDlg.OkDialog);
            WaitForDocumentChange(docChange);
            doc = SkylineWindow.Document;
            refineDocState = (doc.PeptideGroupCount, doc.PeptideCount, doc.PeptideTransitionGroupCount,
                doc.PeptideTransitionCount);
            Assert.AreEqual(graphStates[3], refineDocState);
            RunUI(SkylineWindow.Undo);
        }
    }
}
