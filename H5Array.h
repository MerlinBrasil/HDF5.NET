/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by The HDF Group.                                               *
 * All rights reserved.                                                      *
 *                                                                           *
 * This file is part of HDF5.  The full HDF5 copyright notice, including     *
 * terms governing use, modification, and redistribution, is contained in    *
 * the files COPYING and Copyright.html.  COPYING can be found at the root   *
 * of the source code distribution tree; Copyright.html can be found at the  *
 * root level of an installed copy of the electronic HDF5 document set and   *
 * is linked from the top-level documents page.  It can also be found at     *
 * http://hdfgroup.org/HDF5/doc/Copyright.html.  If you do not have          *
 * access to either file, you may request a copy from help@hdfgroup.org.     *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
#pragma once

namespace HDF5DotNet
{
   generic <class Type>
   public ref class H5Array
   {
      public:
         H5Array(array<Type,1>^ theArray);
         H5Array(array<Type,2>^ theArray);
         H5Array(array<Type,3>^ theArray);
         H5Array(array<Type,4>^ theArray);
         H5Array(array<Type,5>^ theArray);
         H5Array(array<Type,6>^ theArray);
         H5Array(array<Type,7>^ theArray);
         H5Array(array<Type,8>^ theArray);
         H5Array(array<Type,9>^ theArray);
         H5Array(array<Type,10>^ theArray);
         H5Array(array<Type,11>^ theArray);
         H5Array(array<Type,12>^ theArray);
         H5Array(array<Type,13>^ theArray);
         H5Array(array<Type,14>^ theArray);
         H5Array(array<Type,15>^ theArray);
         H5Array(array<Type,16>^ theArray);
         H5Array(array<Type,17>^ theArray);
         H5Array(array<Type,18>^ theArray);
         H5Array(array<Type,19>^ theArray);
         H5Array(array<Type,20>^ theArray);
         H5Array(array<Type,21>^ theArray);
         H5Array(array<Type,22>^ theArray);
         H5Array(array<Type,23>^ theArray);
         H5Array(array<Type,24>^ theArray);
         H5Array(array<Type,25>^ theArray);
         H5Array(array<Type,26>^ theArray);
         H5Array(array<Type,27>^ theArray);
         H5Array(array<Type,28>^ theArray);
         H5Array(array<Type,29>^ theArray);
         H5Array(array<Type,30>^ theArray);
         H5Array(array<Type,31>^ theArray);
         H5Array(array<Type,32>^ theArray);

      internal:
         interior_ptr<Type> getDataAddress();

      private:
         System::Array^ arrayHandle_;
   };
} // end namespace
