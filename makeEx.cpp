/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by The HDF Group (THG).                                       *
 * All rights reserved.                                                    *
 *                                                                         *
 * This file is part of HDF5.  The full HDF5 copyright notice, including   *
 * terms governing use, modification, and redistribution, is contained in  *
 * the files COPYING and Copyright.html.  COPYING can be found at the root *
 * of the source code distribution tree; Copyright.html can be found at the*
 * root level of an installed copy of the electronic HDF5 document set and *
 * is linked from the top-level documents page.  It can also be found at   *
 * http://www.hdfgroup.org/HDF5/doc/Copyright.html.  If you do not have    *
 * access to either file, you may request a copy from help@hdfgroup.org.   *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
#include <iostream>
#include <fstream>
using namespace std;

int main(int argc, char** argv)
{
   const int EXCEPTION_NAMES = 1;
   const int HEADER_FILE_NAME = 2;
   const int CPP_FILE_NAME = 3;
   
   const int N_INPUT_ARGUMENTS = 3;
   
   if (argc != N_INPUT_ARGUMENTS+1)
   {
      cerr << "usage: " << argv[0] 
	   << " <exceptionNames> <headerFile> <cppFile>" << endl;
   }
   else
   {
      ifstream exNames(argv[EXCEPTION_NAMES]);
      ofstream header(argv[HEADER_FILE_NAME]);
      ofstream cpp(argv[CPP_FILE_NAME]);

      cpp << "//This file is automatically generated - do not directly modify."
	  << endl;
      cpp << "#include \"HDFException.h\"" << endl << endl;
      cpp << "#include \"" << argv[HEADER_FILE_NAME] << "\"" << endl;
      cpp << endl;
      cpp << "namespace HDF5DotNet" << endl;
      cpp << "{ " << endl;

      header << 
	"// This file is automatically generated - do not directly modify."
	  << endl;
      header << "#pragma once" << endl;
      header << "#include \"HDFException.h\"" << endl;
      header << "namespace HDF5DotNet" << endl;
      header << "{ " << endl;

      string except;
      while(exNames >> except)
      {
	 // Write the header for this exception.
	 header << "   public ref class " << except << " : public HDFException"
		<< endl;
	 header << "   {" << endl;
         header << "     internal:" << endl;
	 header << "        " << except;
	 header << "(String^ message, unsigned int errorCode);" << endl;
	 header << "   };" << endl << endl;
	 

	 // Write the cpp for this exception
	 cpp << "   " << except << "::" << except
	     << "(String^ message," << endl;
	 cpp << "                                         "
	     << "unsigned int errorCode) : " << endl;
	 cpp << "      HDFException(message, errorCode)" << endl;
	 cpp << "   {" << endl;
	 cpp << "   }" << endl << endl;
      }

      header << "} // close namespace" << endl;
      cpp << "} // close namespace" << endl;
      
      header.close();
      cpp.close();
      exNames.close();
   }
}

