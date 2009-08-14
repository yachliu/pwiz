//
// $Id$
//
//
// Original author: Darren Kessner <Darren.Kessner@cshs.org>
//
// Copyright 2009 Spielberg Family Center for Applied Proteomics
//   Cedars-Sinai Medical Center, Los Angeles, California  90048
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
//


#ifndef _SPECTRUMLISTBASE_HPP_ 
#define _SPECTRUMLISTBASE_HPP_ 


#include "pwiz/data/msdata/MSData.hpp"
#include <stdexcept>


namespace pwiz {
namespace msdata {


/// common functionality for base SpectrumList implementations
class PWIZ_API_DECL SpectrumListBase : public SpectrumList
{
    public:

    /// implementation of SpectrumList
    virtual const boost::shared_ptr<const DataProcessing> dataProcessingPtr() const {return dp_;}

    /// set DataProcessing
    virtual void setDataProcessingPtr(DataProcessingPtr dp) {dp_ = dp;}

    protected:

    DataProcessingPtr dp_;
};


} // namespace msdata 
} // namespace pwiz


#endif // _SPECTRUMLISTBASE_HPP_ 

