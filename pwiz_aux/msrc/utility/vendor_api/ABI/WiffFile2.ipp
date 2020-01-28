//
// $Id: WiffFile2.cpp 10494 2017-02-21 16:53:20Z pcbrefugee $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2009 Vanderbilt University - Nashville, TN 37232
//
// Licensed under Creative Commons 3.0 United States License, which requires:
//  - Attribution
//  - Noncommercial
//  - No Derivative Works
//
// http://creativecommons.org/licenses/by-nc-nd/3.0/us/
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//


#define PWIZ_SOURCE

#ifndef PWIZ_READER_ABI
#error compiler is not MSVC or DLL not available
#else // PWIZ_READER_ABI

#include <msclr/auto_gcroot.h>

using namespace System::Collections::Generic;

#ifdef __INTELLISENSE__
#using <SCIEX.Apis.Data.v1.dll>
#endif

using namespace SCIEX::Apis::Data::v1;
using namespace SCIEX::Apis::Data::v1::Contracts;

namespace pwiz {
namespace vendor_api {
namespace ABI {


class WiffFile2Impl : public WiffFile
{
    public:
    WiffFile2Impl(const std::string& wiffpath);
    ~WiffFile2Impl()
    {
        delete dataReader;
    }

    // prevent multiple Wiff2 files from opening at once, because it currently rewrites DataReader.config every time
    static gcroot<System::Object^> mutex;

    msclr::auto_gcroot<ISampleDataApi^> dataReader;
    mutable gcroot<IList<ISample^>^> allSamples;
    mutable gcroot<ISample^> msSample;
    mutable gcroot<IList<IExperiment^>^> currentSampleExperiments;

    virtual int getSampleCount() const;
    virtual int getPeriodCount(int sample) const;
    virtual int getExperimentCount(int sample, int period) const;
    virtual int getCycleCount(int sample, int period, int experiment) const;

    virtual const vector<string>& getSampleNames() const;

    virtual InstrumentModel getInstrumentModel() const;
    virtual std::string getInstrumentSerialNumber() const;
    virtual IonSourceType getIonSourceType() const;
    virtual blt::local_date_time getSampleAcquisitionTime(int sample, bool adjustToHostTime) const;

    virtual ExperimentPtr getExperiment(int sample, int period, int experiment) const;
    virtual SpectrumPtr getSpectrum(int sample, int period, int experiment, int cycle) const;
    virtual SpectrumPtr getSpectrum(ExperimentPtr experiment, int cycle) const;

    virtual int getADCTraceCount(int sampleIndex) const { return 0; }
    virtual std::string getADCTraceName(int sampleIndex, int traceIndex) const { throw std::out_of_range("WIFF2 does not support ADC traces"); }
    virtual void getADCTrace(int sampleIndex, int traceIndex, ADCTrace& trace) const { throw std::out_of_range("WIFF2 does not support ADC traces"); };

    void setSample(int sample) const;
    void setPeriod(int sample, int period) const;
    void setExperiment(int sample, int period, int experiment) const;
    void setCycle(int sample, int period, int experiment, int cycle) const;

    mutable int currentSampleIndex, currentPeriod, currentExperiment, currentCycle;

    private:
    std::string wiffpath_;
    // on first access, sample names are made unique (giving duplicates a count suffix) and cached
    mutable vector<string> sampleNames;
};

typedef boost::shared_ptr<WiffFile2Impl> WiffFile2ImplPtr;


struct Experiment2Impl : public Experiment
{
    Experiment2Impl(const WiffFile2Impl* WiffFile2, int sample, int period, int experiment);

    virtual int getSampleNumber() const {return sample;}
    virtual int getPeriodNumber() const {return period;}
    virtual int getExperimentNumber() const {return experiment;}

    virtual size_t getSIMSize() const;
    virtual void getSIM(size_t index, Target& target) const;

    virtual size_t getSRMSize() const;
    virtual void getSRM(size_t index, Target& target) const;

    virtual void getSIC(size_t index, pwiz::util::BinaryData<double>& times, pwiz::util::BinaryData<double>& intensities) const;
    virtual void getSIC(size_t index, pwiz::util::BinaryData<double>& times, pwiz::util::BinaryData<double>& intensities,
                        double& basePeakX, double& basePeakY) const;

    virtual bool getHasIsolationInfo() const;
    virtual void getIsolationInfo(int cycle, double& centerMz, double& lowerLimit, double& upperLimit) const;
    virtual void getPrecursorInfo(int cycle, double& centerMz, int& charge) const;

    virtual void getAcquisitionMassRange(double& startMz, double& stopMz) const;
    virtual ScanType getScanType() const;
    virtual ExperimentType getExperimentType() const;
    virtual Polarity getPolarity() const;
    virtual int getMsLevel(int cycle) const;

    virtual double convertCycleToRetentionTime(int cycle) const;
    virtual double convertRetentionTimeToCycle(double rt) const;

    virtual void getTIC(std::vector<double>& times, std::vector<double>& intensities) const;
    virtual void getBPC(std::vector<double>& times, std::vector<double>& intensities) const;

    const WiffFile2Impl* wiffFile_;
    gcroot<IExperiment^> msExperiment;
    int sample, period, experiment;

    ExperimentType experimentType;

    const vector<double>& cycleTimes() const {initializeTIC(); return cycleTimes_;}
    const vector<double>& cycleIntensities() const {initializeTIC(); return cycleIntensities_;}
    //const vector<double>& basePeakMZs() const {initializeBPC(); return basePeakMZs_;}
    //const vector<double>& basePeakIntensities() const {initializeBPC(); return basePeakIntensities_;}

    private:
    void initializeTIC() const;
    void initializeBPC() const;
    mutable bool initializedTIC_;
    mutable bool initializedBPC_;
    mutable pwiz::util::BinaryData<double> cycleTimes_;
    mutable pwiz::util::BinaryData<double> cycleIntensities_;
    //mutable vector<double> basePeakMZs_;
    //mutable pwiz::util::BinaryData<double> basePeakIntensities_;
};

typedef boost::shared_ptr<Experiment2Impl> Experiment2ImplPtr;


struct Spectrum2Impl : public Spectrum
{
    Spectrum2Impl(Experiment2ImplPtr experiment, int cycle);

    virtual int getSampleNumber() const {return experiment->sample;}
    virtual int getPeriodNumber() const {return experiment->period;}
    virtual int getExperimentNumber() const {return experiment->experiment;}
    virtual int getCycleNumber() const {return cycle;}

    virtual int getMSLevel() const;

    virtual bool getHasIsolationInfo() const;
    virtual void getIsolationInfo(double& centerMz, double& lowerLimit, double& upperLimit) const;

    virtual bool getHasPrecursorInfo() const;
    virtual void getPrecursorInfo(double& selectedMz, double& intensity, int& charge) const;

    virtual double getStartTime() const;

    virtual bool getDataIsContinuous() const { return true; }
    virtual size_t getDataSize(bool doCentroid, bool ignoreZeroIntensityPoints = false) const;
    virtual void getData(bool doCentroid, pwiz::util::BinaryData<double>& mz, pwiz::util::BinaryData<double>& intensities, bool ignoreZeroIntensityPoints) const;

    virtual double getSumY() const {return sumY;}
    virtual double getBasePeakX() const {initializeBasePeak(); return bpX;}
    virtual double getBasePeakY() const {initializeBasePeak(); return bpY;}
    virtual double getMinX() const {return minX;}
    virtual double getMaxX() const {return maxX;}

    Experiment2ImplPtr experiment;

    // cache each possible combination of addZeros/doCentroid (probably will only use one, but it avoids need for logic of checking previous setting)
    mutable gcroot<ISpectrum^> msSpectrumCache[2][2];
    mutable gcroot<ISpectrum^> lastSpectrum;

    ISpectrum^ getSpectrumWithOptions(bool addZeros, bool doCentroid) const
    {
        auto& msSpectrum = msSpectrumCache[addZeros ? 1 : 0][doCentroid ? 1 : 0];
        if ((ISpectrum^) msSpectrum == nullptr)
        {
            auto spectrumRequest = experiment->wiffFile_->dataReader->RequestFactory->CreateSpectraReadRequest();
            spectrumRequest->SampleId = experiment->wiffFile_->msSample->Id;
            spectrumRequest->ExperimentId = experiment->msExperiment->Id;
            spectrumRequest->Range->Start = cycle;
            spectrumRequest->Range->End = cycle;

            auto spectraReader = experiment->wiffFile_->dataReader->GetSpectra(spectrumRequest);
            if (spectraReader->MoveNext())
                msSpectrum = spectraReader->GetCurrent();
            else
                msSpectrum = nullptr;
        }

        lastSpectrum = msSpectrum;
        return msSpectrum;
    }

    ISpectrum^ getLastSpectrum() const
    {
        if ((ISpectrum^) lastSpectrum == nullptr)
            lastSpectrum = getSpectrumWithOptions(false, false);
        return lastSpectrum;
    }

    int cycle;

    // data points
    double sumY, minX, maxX;

    // precursor info
    double selectedMz, intensity;
    int charge;

    private:

    mutable double bpX, bpY;
    void initializeBasePeak() const
    {
    }
};

typedef boost::shared_ptr<Spectrum2Impl> Spectrum2ImplPtr;


gcroot<System::Object^> WiffFile2Impl::mutex = gcnew System::Object();

WiffFile2Impl::WiffFile2Impl(const string& wiffpath)
: currentSampleIndex(-1), currentPeriod(-1), currentExperiment(-1), currentCycle(-1), wiffpath_(wiffpath)
{
    try
    {
        System::Threading::Monitor::Enter(mutex);
        auto factory = gcnew DataApiFactory();
        dataReader = factory->CreateSampleDataApi();

        auto sampleRequest = dataReader->RequestFactory->CreateSamplesReadRequest();
        sampleRequest->AbsolutePathToWiffFile = ToSystemString(wiffpath);

        allSamples = gcnew List<ISample^>();

        auto sampleReader = dataReader->GetSamples(sampleRequest);
        while (sampleReader->MoveNext())
            allSamples->Add(sampleReader->GetCurrent());

        System::Threading::Monitor::Exit(mutex);

        // This caused WIFF files where the first sample had been interrupted to
        // throw before they could be successfully constructed, which made investigators
        // unhappy when they were seeking access to later, successfully acquired samples.
        // setSample(1);
    }
    catch (std::exception&) { System::Threading::Monitor::Exit(mutex); throw; }
    catch (System::Exception^ e) { System::Threading::Monitor::Exit(mutex); throw std::runtime_error(trimFunctionMacro(__FUNCTION__, "") + pwiz::util::ToStdString(e->Message)); }
}


int WiffFile2Impl::getSampleCount() const
{
    try {return getSampleNames().size();} CATCH_AND_FORWARD
}

int WiffFile2Impl::getPeriodCount(int sample) const
{
    try
    {
        setSample(sample);
        return 1;
    }
    CATCH_AND_FORWARD
}

int WiffFile2Impl::getExperimentCount(int sample, int period) const
{
    try
    {
        setPeriod(sample, period);        
        return currentSampleExperiments->Count;
    }
    CATCH_AND_FORWARD
}

int WiffFile2Impl::getCycleCount(int sample, int period, int experiment) const
{
    try
    {
        setExperiment(sample, period, experiment);
        IList<IExperiment^>^ unwrappedExperiments = currentSampleExperiments;
        auto currentExperiment = unwrappedExperiments[experiment - 1];

        auto experimentCyclesRequest = dataReader->RequestFactory->CreateExperimentCyclesReadRequest();
        experimentCyclesRequest->SampleId = msSample->Id;
        experimentCyclesRequest->ExperimentId = currentExperiment->Id;

        auto experimentCycles = dataReader->GetExperimentCycles(experimentCyclesRequest);
        return experimentCycles->Cycles->Length;
    }
    CATCH_AND_FORWARD
}

const vector<string>& WiffFile2Impl::getSampleNames() const
{
    try
    {
        if (sampleNames.size() == 0)
        {
            // make duplicate sample names unique by appending the duplicate count
            // e.g. foo, bar, foo (2), foobar, bar (2), foo (3)
            map<string, int> duplicateCountMap;
            IList<ISample^> ^unwrappedAllSamples = allSamples;
            sampleNames.resize(unwrappedAllSamples->Count, "");
            for (int i = 0; i < unwrappedAllSamples->Count; ++i)
                sampleNames[i] = ToStdString(unwrappedAllSamples[i]->SampleName);

            // inexplicably, some files have more samples than sample names;
            // pad the name vector with duplicates of the last sample name;
            // if there are no names, use empty string
            //while (sampleNames.size() < (size_t) batch->SampleCount)
            //    sampleNames.push_back(sampleNames.back());

            for (size_t i=0; i < sampleNames.size(); ++i)
            {
                int duplicateCount = duplicateCountMap[sampleNames[i]]++; // increment after getting current count
                if (duplicateCount)
                    sampleNames[i] += " (" + lexical_cast<string>(duplicateCount+1) + ")";
            }

        }
        return sampleNames;
    }
    CATCH_AND_FORWARD
}

InstrumentModel WiffFile2Impl::getInstrumentModel() const
{
    try
    {
        IInstrumentDetail^ instrumentDetails = nullptr;
        for each (auto instrument in msSample->InstrumentDetails)
        {
            if (instrument->DeviceType == 0) // MS type
            {
                instrumentDetails = instrument;
                break;
            }
        }
        if (instrumentDetails == nullptr)
            throw gcnew Exception("no MS instrument details");

        String^ modelName = instrumentDetails->DeviceModelName->ToUpperInvariant()->Replace(" ", "")->Replace("API", "");
        if (modelName == "UNKNOWN")                 return InstrumentModel_Unknown;
        if (modelName->Contains("2000QTRAP"))       return API2000QTrap; // predicted
        if (modelName->Contains("2000"))            return API2000;
        if (modelName->Contains("2500QTRAP"))       return API2000QTrap; // predicted
        if (modelName->Contains("3000"))            return API3000; // predicted
        if (modelName->Contains("3200QTRAP"))       return API3200QTrap;
        if (modelName->Contains("3200"))            return API3200; // predicted
        if (modelName->Contains("3500QTRAP"))       return API3500QTrap; // predicted
        if (modelName->Contains("4000QTRAP"))       return API4000QTrap;
        if (modelName->Contains("4000"))            return API4000; // predicted
        if (modelName->Contains("QTRAP4500"))       return API4500QTrap;
        if (modelName->Contains("4500"))            return API4500;
        if (modelName->Contains("5000"))            return API5000; // predicted
        if (modelName->Contains("QTRAP5500"))       return API5500QTrap; // predicted
        if (modelName->Contains("5500"))            return API5500; // predicted
        if (modelName->Contains("QTRAP6500"))       return API6500QTrap; // predicted
        if (modelName->Contains("6500"))            return API6500; // predicted
        if (modelName->Contains("QSTARPULSAR"))     return QStarPulsarI; // also covers variants like "API QStar Pulsar i, 0, Qstar"
        if (modelName->Contains("QSTARXL"))         return QStarXL;
        if (modelName->Contains("QSTARELITE"))      return QStarElite;
        if (modelName->Contains("QSTAR"))           return QStar; // predicted
        if (modelName->Contains("TRIPLETOF4600"))   return API4600TripleTOF; // predicted
        if (modelName->Contains("TRIPLETOF5600"))   return API5600TripleTOF;
        if (modelName->Contains("TRIPLETOF6600"))   return API6600TripleTOF; // predicted
        if (modelName->Contains("NLXTOF"))          return NlxTof; // predicted
        if (modelName->Contains("100LC"))           return API100LC; // predicted
        if (modelName->Contains("100"))             return API100; // predicted
        if (modelName->Contains("150MCA"))          return API150MCA; // predicted
        if (modelName->Contains("150EX"))           return API150EX; // predicted
        if (modelName->Contains("165"))             return API165; // predicted
        if (modelName->Contains("300"))             return API300; // predicted
        if (modelName->Contains("350"))             return API350; // predicted
        if (modelName->Contains("365"))             return API365; // predicted
        if (modelName->Contains("X500QTOF"))        return X500QTOF;
        throw gcnew Exception("unknown instrument type: " + instrumentDetails->DeviceModelName);
    }
    CATCH_AND_FORWARD
}

std::string WiffFile2Impl::getInstrumentSerialNumber() const
{
    try
    {
        IInstrumentDetail^ instrumentDetails = nullptr;
        for each (auto instrument in msSample->InstrumentDetails)
        {
            if (instrument->DeviceType == 0) // MS type
            {
                instrumentDetails = instrument;
                break;
            }
        }
        if (instrumentDetails == nullptr)
            throw gcnew Exception("no MS instrument details");

        return ToStdString(instrumentDetails->SerialNumber);
    }
    CATCH_AND_FORWARD
}

IonSourceType WiffFile2Impl::getIonSourceType() const
{
    try {return (IonSourceType) 0;} CATCH_AND_FORWARD
}

blt::local_date_time WiffFile2Impl::getSampleAcquisitionTime(int sample, bool adjustToHostTime) const
{
    try
    {
        setSample(sample);

        System::DateTime acquisitionTime = System::DateTime::Parse(msSample->StartTimestamp);
        bpt::ptime pt(boost::gregorian::date(acquisitionTime.Year, boost::gregorian::greg_month(acquisitionTime.Month), acquisitionTime.Day),
            bpt::time_duration(acquisitionTime.Hour, acquisitionTime.Minute, acquisitionTime.Second, bpt::millisec(acquisitionTime.Millisecond).fractional_seconds()));

        if (adjustToHostTime)
        {
            bpt::time_duration tzOffset = bpt::second_clock::universal_time() - bpt::second_clock::local_time();
            return blt::local_date_time(pt + tzOffset, blt::time_zone_ptr()); // treat time as if it came from host's time zone; actual time zone may not be provided by Sciex
        }
        else
            return blt::local_date_time(pt, blt::time_zone_ptr());
    }
    CATCH_AND_FORWARD
}


ExperimentPtr WiffFile2Impl::getExperiment(int sample, int period, int experiment) const
{
    setExperiment(sample, period, experiment);
    Experiment2ImplPtr msExperiment(new Experiment2Impl(this, sample, period, experiment));
    return msExperiment;
}


Experiment2Impl::Experiment2Impl(const WiffFile2Impl* wiffFile, int sample, int period, int experiment)
: wiffFile_(wiffFile), sample(sample), period(period), experiment(experiment), initializedTIC_(false), initializedBPC_(false)
{
    try
    {
        wiffFile_->setExperiment(sample, period, experiment);

        IList<IExperiment^>^ unwrappedExperiments = wiffFile_->currentSampleExperiments;
        msExperiment = unwrappedExperiments[experiment - 1];

        experimentType = getExperimentType();

    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::initializeTIC() const
{
    if (initializedTIC_)
        return;

    try
    {
        auto experimentTicRequest = wiffFile_->dataReader->RequestFactory->CreateExperimentTicReadRequest();
        experimentTicRequest->SampleId = wiffFile_->msSample->Id;
        experimentTicRequest->ExperimentId = msExperiment->Id;

        auto experimentTic = wiffFile_->dataReader->GetExperimentTic(experimentTicRequest);
        ToBinaryData(experimentTic->XValues, cycleTimes_);
        ToBinaryData(experimentTic->YValues, cycleIntensities_);
        
        initializedTIC_ = true;
    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::initializeBPC() const
{
    if (initializedBPC_)
        return;

    try
    {
        initializedBPC_ = true;
    }
    CATCH_AND_FORWARD
}

size_t Experiment2Impl::getSIMSize() const
{
    try {return 0;} CATCH_AND_FORWARD
}

void Experiment2Impl::getSIM(size_t index, Target& target) const
{
    try
    {
        if (experimentType != SIM)
            return;

        if (index >= 0)
            throw std::out_of_range("[Experiment::getSIM()] index out of range");

        /*SIMMassRange^ transition = (SIMMassRange^) msExperiment->Details->MassRangeInfo[index];

        target.type = TargetType_SIM;
        target.Q1 = transition->Mass;
        target.dwellTime = transition->DwellTime;
        // TODO: store RTWindow?

        // TODO: use NaN to indicate these values should be considered missing?
        target.collisionEnergy = 0;
        target.declusteringPotential = 0;*/
    }
    CATCH_AND_FORWARD
}

size_t Experiment2Impl::getSRMSize() const
{
    try {return 0;} CATCH_AND_FORWARD
}

void Experiment2Impl::getSRM(size_t index, Target& target) const
{
    try
    {
        if (experimentType != MRM)
            return;

        if (index >= 0)
            throw std::out_of_range("[Experiment::getSRM()] index out of range");
    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::getSIC(size_t index, pwiz::util::BinaryData<double>& times, pwiz::util::BinaryData<double>& intensities) const
{
    double x, y;
    getSIC(index, times, intensities, x, y);
}

void Experiment2Impl::getSIC(size_t index, pwiz::util::BinaryData<double>& times, pwiz::util::BinaryData<double>& intensities,
                            double& basePeakX, double& basePeakY) const
{
    try
    {
        if (index >= 0 && index >= 0)
            throw std::out_of_range("[Experiment::getSIC()] index out of range");
    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::getAcquisitionMassRange(double& startMz, double& stopMz) const
{
    try
    {
        if (experimentType != MRM)
        {
            auto massRanges = (cli::array<IMassRange^>^) msExperiment->MassRanges;
            startMz = massRanges[0]->Start;
            stopMz = massRanges[0]->End;
        }
        else
            startMz = stopMz = 0;
    }
    CATCH_AND_FORWARD
}

ScanType Experiment2Impl::getScanType() const
{
    try 
    {
        return ScanType::FullScan;

    } CATCH_AND_FORWARD
}

ExperimentType Experiment2Impl::getExperimentType() const
{
    try
    {
        if (msExperiment->ScanType == "TOFMS")
            return ExperimentType::MS;
        if (msExperiment->ScanType == "TOFMSMS")
            return ExperimentType::Product;

        return ExperimentType::MS;

    } CATCH_AND_FORWARD
}

Polarity Experiment2Impl::getPolarity() const
{
    try {return msExperiment->IsPositivePolarityScan ? Positive : Negative;} CATCH_AND_FORWARD
}

int Experiment2Impl::getMsLevel(int cycle) const
{
    try { return msExperiment->MsLevel; } CATCH_AND_FORWARD
}

double Experiment2Impl::convertCycleToRetentionTime(int cycle) const
{
    try {return cycleTimes()[cycle - 1];} CATCH_AND_FORWARD
}

double Experiment2Impl::convertRetentionTimeToCycle(double rt) const
{
    try {return 0;} CATCH_AND_FORWARD
}

void Experiment2Impl::getTIC(std::vector<double>& times, std::vector<double>& intensities) const
{
    try
    {
        times = cycleTimes();
        intensities = cycleIntensities();
    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::getBPC(std::vector<double>& times, std::vector<double>& intensities) const
{
    try
    {
        //times = cycleTimes();
        //intensities = basePeakIntensities();
    }
    CATCH_AND_FORWARD
}

bool Experiment2Impl::getHasIsolationInfo() const
{
    return experimentType == Product;
}

void Experiment2Impl::getIsolationInfo(int cycle, double& centerMz, double& lowerLimit, double& upperLimit) const
{
    if (!getHasIsolationInfo())
        return;

    try
    {
    }
    CATCH_AND_FORWARD
}

void Experiment2Impl::getPrecursorInfo(int cycle, double& centerMz, int& charge) const
{
    if (!getHasIsolationInfo())
        return;

    try
    {
    }
    CATCH_AND_FORWARD
}


Spectrum2Impl::Spectrum2Impl(Experiment2ImplPtr experiment, int cycle)
: experiment(experiment), cycle(cycle), selectedMz(0), bpY(-1), bpX(-1)
{
    try
    {
        sumY = experiment->cycleIntensities()[cycle-1];
        //minX = experiment->; // TODO Mass range?
        //maxX = spectrum->MaximumXValue;

        intensity = 0;
    }
    CATCH_AND_FORWARD
}

int Spectrum2Impl::getMSLevel() const
{
    try { return experiment->getMsLevel(cycle) == 0 ? 1 : experiment->getMsLevel(cycle); } CATCH_AND_FORWARD
}

bool Spectrum2Impl::getHasIsolationInfo() const { return experiment->getHasIsolationInfo(); }

void Spectrum2Impl::getIsolationInfo(double& centerMz, double& lowerLimit, double& upperLimit) const
{
    if (!getHasIsolationInfo())
        return;

    try
    {
        auto isolationWindow = getLastSpectrum()->Precursor->IsolationWindow;
        centerMz = isolationWindow->IsolationWindowTarget;
        lowerLimit = centerMz - isolationWindow->LowerOffset;
        upperLimit = centerMz + isolationWindow->UpperOffset;
    }
    CATCH_AND_FORWARD
}

bool Spectrum2Impl::getHasPrecursorInfo() const
{
    return selectedMz != 0;
}

void Spectrum2Impl::getPrecursorInfo(double& selectedMz, double& intensity, int& charge) const
{
    try
    {
        auto precursor = getLastSpectrum()->Precursor;
        selectedMz = precursor->IsolationWindow->IsolationWindowTarget;
        intensity = 0;
        charge = precursor->PrecursorChargeState;
    }
    CATCH_AND_FORWARD
}

double Spectrum2Impl::getStartTime() const
{
    return getLastSpectrum()->ScanStartTime;
}

size_t Spectrum2Impl::getDataSize(bool doCentroid, bool ignoreZeroIntensityPoints) const
{
    try
    {
        return getSpectrumWithOptions(!ignoreZeroIntensityPoints, doCentroid)->XValues->Length;
    }
    CATCH_AND_FORWARD
}

void Spectrum2Impl::getData(bool doCentroid, pwiz::util::BinaryData<double>& mz, pwiz::util::BinaryData<double>& intensities, bool ignoreZeroIntensityPoints) const
{
    try
    {
        auto spectrum = getSpectrumWithOptions(!ignoreZeroIntensityPoints, doCentroid);
        ToBinaryData(spectrum->XValues, mz);
        ToBinaryData(spectrum->YValues, intensities);
    }
    CATCH_AND_FORWARD
}


SpectrumPtr WiffFile2Impl::getSpectrum(int sample, int period, int experiment, int cycle) const
{
    try
    {
        ExperimentPtr msExperiment = getExperiment(sample, period, experiment);
        return getSpectrum(msExperiment, cycle);
    }
    CATCH_AND_FORWARD
}

SpectrumPtr WiffFile2Impl::getSpectrum(ExperimentPtr experiment, int cycle) const
{
    Spectrum2ImplPtr spectrum(new Spectrum2Impl(boost::static_pointer_cast<Experiment2Impl>(experiment), cycle));
    return spectrum;
}


void WiffFile2Impl::setSample(int sample) const
{
    try
    {
        if (sample != currentSampleIndex)
        {
            //this->msSample = static_cast<IList<ISampleInformation^>^>(allSamples)[0];
            IList<ISample^>^ unwrappedAllSamples = allSamples;
            this->msSample = (ISample^)unwrappedAllSamples[sample - 1];

            auto experimentRequest = dataReader->RequestFactory->CreateExperimentsReadRequest();
            experimentRequest->SampleId = msSample->Id;

            currentSampleExperiments = gcnew List<IExperiment^>();

            auto experimentReader = dataReader->GetExperiments(experimentRequest);
            while (experimentReader->MoveNext())
                currentSampleExperiments->Add(experimentReader->GetCurrent());

            currentSampleIndex = sample;
            currentPeriod = currentExperiment = currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFile2Impl::setPeriod(int sample, int period) const
{
    try
    {
        setSample(sample);

        if (period != currentPeriod)
        {
            currentPeriod = period;
            currentExperiment = currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFile2Impl::setExperiment(int sample, int period, int experiment) const
{
    try
    {
        setPeriod(sample, period);

        if (experiment != currentExperiment)
        {
            currentExperiment = experiment;
            currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFile2Impl::setCycle(int sample, int period, int experiment, int cycle) const
{
    try
    {
        setExperiment(sample, period, experiment);

        if (cycle != currentCycle)
        {
            currentCycle = cycle;
        }
    }
    CATCH_AND_FORWARD
}


} // ABI
} // vendor_api
} // pwiz

#endif
