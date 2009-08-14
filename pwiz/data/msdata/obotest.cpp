//
// $Id$
//
//
// Original author: Darren Kessner <Darren.Kessner@cshs.org>
//
// Copyright 2007 Spielberg Family Center for Applied Proteomics
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


#include "obo.hpp"
#include "pwiz/utility/misc/unit.hpp"
#include <boost/filesystem/operations.hpp>
#include <iostream>
#include <fstream>
#include <stdexcept>
#include <cstring>


using namespace std;
using namespace pwiz;
using namespace pwiz::msdata;
using namespace pwiz::util;


ostream* os_ = 0;


const char* oboText_ = 
    "format-version: 1.0\n"
    "date: 01:10:2007 23:46\n"
    "saved-by: deutsch\n"
    "auto-generated-by: OBO-Edit 1.101\n"
    "default-namespace: PSI-MS\n"
    "\n"
    "[Term]\n"
    "id: MS:0000000\n"
    "name: MZ controlled vocabularies\n"
    "def: \"MZ controlled vocabularies.\" [PSI:MS]\n"
    "\n"
    "[Term]\n"
    "id: MS:0000001\n"
    "name: sample number\n"
    "def: \"A reference number relevant to the sample under study.\" [PSI:MS]\n"
    "relationship: part_of MS:1000548 ! sample attribute\n"
    "\n"
    "[Term]\n"
    "id: MS:0000011\n"
    "name: mass resolution\n"
    "def: \"The maximum m/z value at which two peaks can be resolved, according to one of the standard measures.\" [PSI:MS]\n"
    "is_a: MS:1000503 ! scan attribute\n"
    "\n"
    "[Term]\n"
    "id: MS:1000025\n"
    "name: magnetic field strength\n"
    "def: \"A property of space that produces a force on a charged particle equal to qv x B where q is the particle charge and v its velocity.\" [PSI:MS]\n"
    "related_synonym: \"Magnetic Field\" []\n"
    "exact_synonym: \"B\" []\n"
    "is_a: MS:1000480 ! mass analyzer attribute\n"
    "\n"
    "[Term]\n"
    "id: MS:1000030\n"
    "name: vendor\n"
    "def: \"Name of instrument vendor, replaced by MS:1000031 Model From Vendor.\" [PSI:MS]\n"
    "is_obsolete: true\n"
    "\n"
    "[Term]\n"
    "id: MS:1000035\n"
    "name: obsolete by definition\n"
    "def: \"OBSOLETE description\" [PSI:MS]\n"
    "\n"
    "[Term]\n"
    "id: MS:1001272\n"
    "name: (?<=R)(?\\!P)\n"
    "\n"
    "[Term]\n"
    "id: MS:1001280\n"
    "name: accuracy\n"
    "def: \"Accuracy is the degree of conformity of a measured mass to its actual value.\" [PSI:MS]\n"
    "xref: value-type:xsd\\:float \"The allowed value-type for this CV term.\"\n"
    "is_a: MS:1000480 ! mass analyzer attribute\n"
    "relationship: has_units MS:1000040 ! m/z\n"
    "relationship: has_units UO:0000169 ! parts per million\n"
    "\n"
    "[Term]\n"
    "id: MS:1001303\n"
    "name: Arg-C\n"
    "is_a: MS:1001045 ! cleavage agent name\n"
    "relationship: has_regexp MS:1001272 ! (?<=R)(?!P)\n"
    "\n"
    // OBO format 1.2
    "[Term]\n"
    "id: MS:2000025\n"
    "name: magnetic field strength\n"
    "def: \"A property of space that produces a force on a charged particle equal to qv x B where q is the particle charge and v its velocity.\" [PSI:MS]\n"
    "synonym: \"B\" EXACT []\n"
    "synonym: \"Magnetic Field\" RELATED []\n"
    "is_a: MS:1000480 ! mass analyzer attribute\n"
    "\n"
    "[Term]\n"
    "id: MS:9999999\n"
    "name: unit\n"
    "namespace: unit.ontology\n"
    "def: \"description\" [ignore this Wikipedia:Wikipedia \"http://www.wikipedia.org/\"]\n"
    "\n"
;


void test()
{
    const string& filename = "obotest_temp.txt";
    ofstream temp(filename.c_str());
    temp << oboText_ << endl; 
    temp.close();

    OBO obo(filename);

    if (os_) *os_ << obo << endl; 
   
    unit_assert(obo.filename == filename);    
    unit_assert(obo.header.size() == 5); 
    unit_assert(obo.prefix == "MS");
    unit_assert(obo.terms.size() == 11); // including obsolete terms

    set<Term>::const_iterator term = obo.terms.begin();
    unit_assert(term->prefix == "MS");
    unit_assert(term->id == 0);
    unit_assert(term->name == "MZ controlled vocabularies");
    unit_assert(term->def == "MZ controlled vocabularies.");
    unit_assert(term->parentsPartOf.empty());
    unit_assert(term->parentsIsA.empty());

    ++term;
    unit_assert(term->id == 1);
    unit_assert(term->name == "sample number");
    unit_assert(term->parentsPartOf.size() == 1);
    unit_assert(term->parentsPartOf[0] == 1000548);
 
    ++term;
    unit_assert(term->id == 11);
    unit_assert(term->name == "mass resolution");
    unit_assert(term->parentsIsA.size() == 1);
    unit_assert(term->parentsIsA[0] == 1000503);

    ++term;
    unit_assert(term->id == 1000025);
    unit_assert(term->exactSynonyms.size() == 1);
    unit_assert(term->exactSynonyms[0] == "B");

    ++term;
    unit_assert(term->id == 1000030);
    unit_assert(term->isObsolete);

    ++term;
    unit_assert(term->id == 1000035);
    unit_assert(term->isObsolete);

    // test unescaping "\!"
    ++term;
    unit_assert(term->id == 1001272);
    unit_assert(term->name == "(?<=R)(?!P)");

    // test other relationships
    ++term;
    unit_assert(term->id == 1001280);
    unit_assert(term->relations.size() == 2);
    unit_assert(term->relations.begin()->first == "has_units");
    unit_assert(term->relations.begin()->second.first == "MS");
    unit_assert(term->relations.begin()->second.second == 1000040);
    unit_assert(term->relations.rbegin()->second.first == "UO");
    unit_assert(term->relations.rbegin()->second.second == 169);

    ++term;
    unit_assert(term->id == 1001303);
    unit_assert(term->name == "Arg-C");
    unit_assert(term->relations.size() == 1);
    unit_assert(term->relations.begin()->first == "has_regexp");
    unit_assert(term->relations.begin()->second.first == "MS");
    unit_assert(term->relations.begin()->second.second == 1001272);

    // test term with OBO 1.2 synonym format
    ++term;
    unit_assert(term->id == 2000025);
    unit_assert(term->exactSynonyms.size() == 1);
    unit_assert(term->exactSynonyms[0] == "B");

    // test term with [stuff to ignore]
    ++term;
    unit_assert(term->id == 9999999);
    unit_assert(term->def == "description");
 
    boost::filesystem::remove(filename); 
}


int main(int argc, char* argv[])
{
    try
    {
        if (argc>1 && !strcmp(argv[1],"-v")) os_ = &cout;
        test();
        return 0;
    }
    catch (exception& e)
    {
        cerr << e.what() << endl;
    }
    catch (...)
    {
        cerr << "Caught unknown exception.\n";
    }

    return 1; 
}


