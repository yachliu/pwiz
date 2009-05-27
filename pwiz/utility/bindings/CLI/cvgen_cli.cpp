//
// cvgen.cpp
//
// 
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2008 Spielberg Family Center for Applied Proteomics
//   Cedars Sinai Medical Center, Los Angeles, California  90048
// Copyright 2008 Vanderbilt University - Nashville, TN 37232
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


#include "../../../data/msdata/obo.hpp"
#include "boost/filesystem/path.hpp"
#include "boost/filesystem/fstream.hpp"
#include <iostream>
#include <iomanip>
#include <fstream>
#include <iterator>
#include <algorithm>
#include <map>


using namespace std;
using namespace pwiz;
using namespace pwiz::msdata;
namespace bfs = boost::filesystem;


//
// This program selectively parses OBO format controlled vocabulary files 
// and generates C++/CLI code for the pwiz CLI binding (one hpp file).
//


void writeCopyright(ostream& os, const string& filename)
{
    os << "//\n"
       << "// " << filename << endl
       << "//\n"
          "//\n"
          "// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>\n"
          "//\n"
          "// Copyright 2008 Spielberg Family Center for Applied Proteomics\n"
          "//   Cedars Sinai Medical Center, Los Angeles, California  90048\n"
          "// Copyright 2008 Vanderbilt University - Nashville, TN 37232\n"
          "//\n"
          "// Licensed under the Apache License, Version 2.0 (the \"License\");\n"
          "// you may not use this file except in compliance with the License.\n"
          "// You may obtain a copy of the License at\n"
          "//\n"
          "// http://www.apache.org/licenses/LICENSE-2.0\n"
          "//\n"
          "// Unless required by applicable law or agreed to in writing, software\n"
          "// distributed under the License is distributed on an \"AS IS\" BASIS,\n"
          "// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\n"
          "// See the License for the specific language governing permissions and\n"
          "// limitations under the License.\n"
          "//\n"
          "// This file was generated by cvgen_cli.\n"
          "//\n\n\n";
}


string includeGuardString(const string& basename)
{
    string includeGuard = basename;
    transform(includeGuard.begin(), includeGuard.end(), includeGuard.begin(), (int(*)(int))toupper);
    return "_" + includeGuard + "_HPP_CLI_";
}


void namespaceBegin(ostream& os, const string& name)
{
    os << "namespace pwiz {\n"
       << "namespace CLI {\n\n\n";
}


void namespaceEnd(ostream& os, const string& name)
{
    os << "} // namespace CLI\n"
       << "} // namespace pwiz\n\n\n";
}


inline char toAllowableChar(char a)
{
    return isalnum(a) ? a : '_';
}


string enumName(const string& prefix, const string& name)
{
    string result = name;
    transform(result.begin(), result.end(), result.begin(), toAllowableChar);
    result = prefix + "_" + result;
    return result;
}


string enumName(const Term& term)
{
    return enumName(term.prefix, term.name);
}


const size_t enumBlockSize_ = 100000000;


size_t enumValue(const Term& term, size_t index)
{
    return term.id + (enumBlockSize_ * index);
}


void writeHpp(const vector<OBO>& obos, const string& basename, const bfs::path& outputDir)
{
    string filename = basename + ".hpp";
    bfs::path filenameFullPath = outputDir / filename;
    bfs::ofstream os(filenameFullPath, ios::binary);

    writeCopyright(os, filename);

    string includeGuard = includeGuardString(basename);
    os << "#ifndef " << includeGuard << endl
       << "#define " << includeGuard << "\n\n\n"
       << "#include \"../../../data/msdata/cv.hpp\"\n"
       << "#include \"SharedCLI.hpp\"\n"
       << "\n\n";

    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)
    {
        os << "// [" << obo->filename << "]\n";
        
        for (vector<string>::const_iterator it=obo->header.begin(); it!=obo->header.end(); ++it)
            os << "//   " << *it << endl;

        os << "//\n";
    }
    os << "\n\n";

    namespaceBegin(os, basename);

    os << "/// <summary>enumeration of controlled vocabulary (CV) terms, generated from OBO file(s)</summary>\n" 
          "public enum class CVID\n{\n"
          "    CVID_Unknown = -1";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
    {
        os << ",\n\n"
           << "    /// <summary>" << it->name << ": " << it->def << "</summary>\n"
           << "    " << enumName(*it) << " = " << enumValue(*it, obo-obos.begin());
        
        if (obo->prefix == "MS") // add synonyms for PSI-MS only
        {
            for (vector<string>::const_iterator syn=it->exactSynonyms.begin(); 
                 syn!=it->exactSynonyms.end(); ++syn)
            {
                os << ",\n\n"
                   << "    /// <summary>" << it->name << ": " << it->def << "</summary>\n"
                   << "    " << enumName(it->prefix, *syn) << " = " << enumName(*it);
            }
        }
    }
    os << "\n}; // enum CVID\n\n\n"; 

    os << "/// <summary>\n"
       << "/// A list of enumerated CVIDs (CV terms); implements IList&lt;CVID&gt;\n"
       << "/// </summary>\n"
       << "DEFINE_STD_VECTOR_WRAPPER_FOR_VALUE_TYPE(CVIDList, pwiz::CVID, CVID, NATIVE_VALUE_TO_CLI, CLI_VALUE_TO_NATIVE_VALUE);\n"
       << "\n"
       << "/// <summary>\n"
       << "/// A list of enumerated System.Strings; implements IList&lt;System.String&gt;\n"
       << "/// </summary>\n"
       << "DEFINE_STD_VECTOR_WRAPPER_FOR_REFERENCE_TYPE(StringList, std::string, System::String, STD_STRING_TO_CLI_STRING, CLI_STRING_TO_STD_STRING);\n"
       << "\n"
       << "/// <summary>\n"
       << "/// A utility class for getting detailed information about a particular CVID (CV term)\n"
       << "/// </summary>\n"
       << "public ref class CVTermInfo\n"
       << "{\n"
       << "    DEFINE_INTERNAL_BASE_CODE(CVTermInfo, pwiz::CVTermInfo);\n"
       << "    public:\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns the CVID corresponding this CVTermInfo instance describes\n"
       << "    /// <para>- note: for PSI-MS terms, the CVID always corresponds with the accession number\n"
       << "    /// </summary>\n"
       << "    property CVID cvid { CVID get() {return (CVID) base_->cvid;} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns the accession id of the term in the form: \"prefix:number\", e.g. \"MS:1000001\"\n"
       << "    /// </summary>\n"
       << "    property System::String^ id { System::String^ get() {return gcnew System::String(base_->id.c_str());} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// the full name of the term, e.g. \"sample number\"\n"
       << "    /// </summary>\n"
       << "    property System::String^ name { System::String^ get() {return gcnew System::String(base_->name.c_str());} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns the full text definition of the term\n"
       << "    /// </summary>\n"
       << "    property System::String^ def { System::String^ get() {return gcnew System::String(base_->def.c_str());} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns a list of terms which this term has an IS_A relationship with\n"
       << "    /// </summary>\n"
       << "    property CVIDList^ parentsIsA { CVIDList^ get() {return gcnew CVIDList(&base_->parentsIsA);} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns a list of terms which this term has a PART_OF relationship with\n"
       << "    /// </summary>\n"
       << "    property CVIDList^ parentsPartOf { CVIDList^ get() {return gcnew CVIDList(&base_->parentsPartOf);} }\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns a list of term names synonymous with this term\n"
       << "    /// </summary>\n"
       << "    property StringList^ exactSynonyms { StringList^ get() {return gcnew StringList(&base_->exactSynonyms);} }\n"
       << "\n"
       << "    CVTermInfo() : base_(new pwiz::CVTermInfo()) {}\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns CV term info for the specified CVID\n"
       << "    /// </summary>\n"
       << "    CVTermInfo(CVID cvid) : base_(new pwiz::CVTermInfo(pwiz::CVTermInfo((pwiz::CVID) cvid))) {}\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns CV term info for the specified id in the form: \"prefix:number\"\n"
       << "    /// </summary>\n"
       << "    CVTermInfo(System::String^ id) : base_(new pwiz::CVTermInfo(pwiz::CVTermInfo(ToStdString(id)))) {}\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns the shortest synonym from exactSynonyms()\n"
       << "    /// </summary>\n"
       << "    System::String^ shortName() {return gcnew System::String(base_->shortName().c_str());}\n"
       << "\n"
       << "    /// <summary>\n"
       << "    /// returns the prefix of the id, e.g. \"MS\" for PSI-MS terms\n"
       << "    /// </summary>\n"
       << "    System::String^ prefix() {return gcnew System::String(base_->prefix().c_str());}\n"
       << "};\n\n\n";

    namespaceEnd(os, basename);

    os << "#endif // " << includeGuard << "\n\n\n";
}


void writeCpp(const vector<OBO>& obos, const string& basename, const bfs::path& outputDir)
{
    string filename = basename + ".cpp";
    bfs::path filenameFullPath = outputDir / filename;
    bfs::ofstream os(filenameFullPath, ios::binary);

    writeCopyright(os, filename);

    os << "#define PWIZ_SOURCE\n\n"
       << "#include \"" << basename << ".hpp\"\n"
       << "#include \"utility/misc/String.hpp\"\n"
       << "#include \"utility/misc/Container.hpp\"\n"
       << "#include \"utility/misc/Exception.hpp\"\n"
       << "\n\n";

    namespaceBegin(os, basename);

    os << "namespace {\n\n\n";

    os << "struct TermInfo\n"
          "{\n"
          "    CVID cvid;\n"
          "    const char* id;\n"
          "    const char* name;\n"
          "    const char* def;\n"
          "};\n\n\n";

    os << "const TermInfo termInfos_[] =\n{\n";
    os << "    {CVID_Unknown, \"??:0000000\", \"CVID_Unknown\", \"CVID_Unknown\"},\n";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
        os << "    {" << enumName(*it) << ", "
           << "\"" << it->prefix << ":" << setw(7) << setfill('0') << it->id << "\", "
           << "\"" << it->name << "\", " 
           << "\"" << it->def << "\""
           << "},\n";
    os << "}; // termInfos_\n\n\n";

    os << "const size_t termInfosSize_ = sizeof(termInfos_)/sizeof(TermInfo);\n\n\n";

    os << "struct CVIDPair\n"
          "{\n"
          "    CVID first;\n"
          "    CVID second;\n"
          "};\n\n\n";

    // create a term map for each OBO

    vector< map<Term::id_type, const Term*> > termMaps(obos.size());
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)    
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
        termMaps[obo-obos.begin()][it->id] = &*it;

    os << "CVIDPair relationsIsA_[] =\n{\n";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)    
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
    for (Term::id_list::const_iterator jt=it->parentsIsA.begin(); jt!=it->parentsIsA.end(); ++jt)
        os << "    {" << enumName(*it) << ", " 
           << enumName(*termMaps[obo-obos.begin()][*jt]) << "},\n";
    os << "}; // relationsIsA_\n\n\n";

    os << "const size_t relationsIsASize_ = sizeof(relationsIsA_)/sizeof(CVIDPair);\n\n\n";

    os << "CVIDPair relationsPartOf_[] =\n{\n";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)    
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
    for (Term::id_list::const_iterator jt=it->parentsPartOf.begin(); jt!=it->parentsPartOf.end(); ++jt)
        os << "    {" << enumName(*it) << ", " 
           << enumName(*termMaps[obo-obos.begin()][*jt]) << "},\n";
    os << "}; // relationsPartOf_\n\n\n";

    os << "const size_t relationsPartOfSize_ = sizeof(relationsPartOf_)/sizeof(CVIDPair);\n\n\n";

    os << "struct CVIDStringPair\n"
          "{\n"
          "    CVID first;\n"
          "    const char* second;\n"
          "};\n\n\n";

    os << "CVIDStringPair relationsExactSynonym_[] =\n"
       << "{\n"
       << "    {CVID_Unknown, \"Unknown\"},\n";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)    
    for (vector<Term>::const_iterator it=obo->terms.begin(); it!=obo->terms.end(); ++it)
    for (vector<string>::const_iterator jt=it->exactSynonyms.begin(); jt!=it->exactSynonyms.end(); ++jt)
        os << "    {" << enumName(*it) << ", " 
           << "\"" << *jt << "\"" << "},\n";
    os << "}; // relationsExactSynonym_\n\n\n";

    os << "const size_t relationsExactSynonymSize_ = sizeof(relationsExactSynonym_)/sizeof(CVIDStringPair);\n\n\n";

    os << "bool initialized_ = false;\n"
          "map<CVID,CVTermInfo> infoMap_;\n"
          "vector<CVID> cvids_;\n"
          "\n\n";

    os << "void initialize()\n"
          "{\n"
          "    for (const TermInfo* it=termInfos_; it!=termInfos_+termInfosSize_; ++it)\n" 
          "    {\n"
          "        CVTermInfo temp;\n"
          "        temp.cvid = it->cvid;\n"
          "        temp.id = it->id;\n"
          "        temp.name = it->name;\n"
          "        temp.def = it->def;\n"
          "        infoMap_[temp.cvid] = temp;\n"
          "        cvids_.push_back(it->cvid);\n"
          "    }\n"
          "\n"
          "    for (const CVIDPair* it=relationsIsA_; it!=relationsIsA_+relationsIsASize_; ++it)\n"
          "        infoMap_[it->first].parentsIsA.push_back(it->second);\n"
          "\n"
          "    for (const CVIDPair* it=relationsPartOf_; it!=relationsPartOf_+relationsPartOfSize_; ++it)\n"
          "        infoMap_[it->first].parentsPartOf.push_back(it->second);\n"
          "\n"
          "    for (const CVIDStringPair* it=relationsExactSynonym_; it!=relationsExactSynonym_+relationsExactSynonymSize_; ++it)\n"
          "        infoMap_[it->first].exactSynonyms.push_back(it->second);\n"
          "\n"
          "    initialized_ = true;\n"
          "}\n\n\n";

    os << "const char* oboPrefixes_[] =\n"
          "{\n";
    for (vector<OBO>::const_iterator obo=obos.begin(); obo!=obos.end(); ++obo)
        os << "    \"" << obo->prefix << "\",\n";
    os << "};\n\n\n";

    os << "const size_t oboPrefixesSize_ = sizeof(oboPrefixes_)/sizeof(const char*);\n\n\n"

          "const size_t enumBlockSize_ = " << enumBlockSize_ << ";\n\n\n"

          "struct StringEquals\n"
          "{\n"
          "    bool operator()(const string& yours) {return mine==yours;}\n"
          "    string mine;\n"
          "    StringEquals(const string& _mine) : mine(_mine) {}\n"
          "};\n\n\n";

    os << "} // namespace\n\n\n";

    os << "PWIZ_API_DECL const string& CVTermInfo::shortName() const\n"
          "{\n"
          "    const string* result = &name;\n"
          "    for (vector<string>::const_iterator it=exactSynonyms.begin(); it!=exactSynonyms.end(); ++it)\n"
          "        if (result->size() > it->size())\n"
          "            result = &*it;\n"
          "    return *result;\n"
          "}\n\n\n";

    os << "PWIZ_API_DECL string CVTermInfo::prefix() const\n"
          "{\n"
          "    return id.substr(0, id.find_first_of(\":\"));\n"
          "}\n\n\n";

    os << "PWIZ_API_DECL const CVTermInfo& cvTermInfo(CVID cvid)\n"
          "{\n"
          "   if (!initialized_) initialize();\n"
          "   return infoMap_[cvid];\n"
          "}\n\n\n";

    os << "inline unsigned int stringToCVID(const std::string& str)\n"
          "{\n"
          "    errno = 0;\n"
          "    const char* stringToConvert = str.c_str();\n"
          "    const char* endOfConversion = stringToConvert;\n"
          "    unsigned int value = (unsigned int) strtoul (stringToConvert, const_cast<char**>(&endOfConversion), 10);\n"
          "    if (( value == 0u && stringToConvert == endOfConversion) || // error: conversion could not be performed\n"
          "        errno != 0 ) // error: overflow or underflow\n"
          "        throw bad_lexical_cast();\n"
          "    return value;\n"
          "}\n\n\n";

    os << "PWIZ_API_DECL const CVTermInfo& cvTermInfo(const string& id)\n"
          "{\n"
          "    if (!initialized_) initialize();\n"
          "    CVID cvid = CVID_Unknown;\n"
          "\n"
          "    vector<string> tokens;\n"
          "    tokens.reserve(2);\n"
          "    bal::split(tokens, id, bal::is_any_of(\":\"));\n"
          "    if (tokens.size() != 2)\n"
          "        throw runtime_error(\"[cvinfo] Error splitting id \\\"\" + id + \"\\\" into prefix and numeric components\");\n"
          "    const string& prefix = tokens[0];\n"
          "    const string& cvidStr = tokens[1];\n"
          "\n"
          "    const char** it = find_if(oboPrefixes_, oboPrefixes_+oboPrefixesSize_,\n"
          "                              StringEquals(prefix.c_str()));\n"
          "\n"
          "    if (it != oboPrefixes_+oboPrefixesSize_)\n"
          "       cvid = (CVID)((it-oboPrefixes_)*enumBlockSize_ + stringToCVID(cvidStr));\n"
          "\n"
          "    return infoMap_[cvid];\n"
          "}\n\n\n";

    os << "PWIZ_API_DECL bool cvIsA(CVID child, CVID parent)\n"
          "{\n"
          "    if (child == parent) return true;\n"
          "    const CVTermInfo& info = cvTermInfo(child);\n"
          "    for (CVTermInfo::id_list::const_iterator it=info.parentsIsA.begin(); it!=info.parentsIsA.end(); ++it)\n"
          "        if (cvIsA(*it,parent)) return true;\n"
          "    return false;\n"
          "}\n\n\n";

    os << "PWIZ_API_DECL const vector<CVID>& cvids()\n"
          "{\n"
          "   if (!initialized_) initialize();\n"
          "   return cvids_;\n"
          "}\n\n\n";

    namespaceEnd(os, basename);
}


void generateFiles(const vector<OBO>& obos, const string& basename, const bfs::path& outputDir)
{
    writeHpp(obos, basename, outputDir);
    //writeCpp(obos, basename, outputDir);
}


int main(int argc, char* argv[])
{
    if (argc < 2)
    {
        cout << "Usage: cvgen file.obo [...]\n";
        cout << "Parse input file(s) and output cv.hpp and cv.cpp.\n";
        return 1;
    }

    try
    {
        bfs::path exeDir(bfs::path(argv[0]).branch_path());

        vector<OBO> obos;
        for (int i=1; i<argc; i++)
            obos.push_back(OBO(argv[i]));

        generateFiles(obos, "cv", exeDir);

        return 0;
    }
    catch (exception& e)
    {
        cerr << "Caught exception: " << e.what() << endl;
    }
    catch (...)
    {
        cerr << "Caught unknown exception.\n";
    }

    return 1; 
}

