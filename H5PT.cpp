/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by The HDF Group (THG)                                        *
 * Copyright by Jesse Lai.                                                 *
 * All rights reserved.                                                    *
 *                                                                         *
 * This file is part of HDF5. The full HDF5 copyright notice, including    *
 * terms governing use, modification, and redistribution, is contained in  *
 * the files COPYING and Copyright.html. COPYING can be found at the root  *
 * of the source code distribution tree; Copyright.html can be found at the*
 * root level of an installed copy of the electronic HDF5 document set and *
 * is linked from the top-level documents page. It can also be found at    *
 * http://www.hdfgroup.org/HDF5/doc/Copyright.html. If you do not have     *
 * access to either file, you may request a copy from help@hdfgroup.org.   *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
// This is the main DLL file.
#include "stdafx.h"
#include <cassert>
using namespace std;
using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Text;

//#include "H5Common.h"
#include "H5Array.h"
#include "H5PT.h"

#pragma region Externals

// H5PTappend
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTappend(
   hid_t locId,
   size_t packetCount,
   void* data);

// H5PTclose
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTclose(
   hid_t locId);

// H5PTcreate_fl
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" hid_t _cdecl H5PTcreate_fl(
   hid_t locId,
   [MarshalAs(UnmanagedType::LPStr)]String^ dataSetName,
   hid_t dataTypeId,
   hsize_t chunkSize,
   int compression);

// H5PTcreate_index
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTcreate_index(
   hid_t locId);

// H5PTcreate_vl
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" hid_t _cdecl H5PTcreate_vl(
   hid_t locId,
   [MarshalAs(UnmanagedType::LPStr)]String^ dataSetName,
   hsize_t chunkSize);

// H5PTfree_vlen_readbuff
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTfree_vlen_readbuff(
   hid_t locId,
   size_t bufferLength,
   void* buffer);

// H5PTget_index
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTget_index(
   hid_t locId,
   hsize_t *index);

// H5PTget_next
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTget_next(
   hid_t locId,
   size_t packetCount,
   void* data);

// H5PTget_num_packets
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTget_num_packets(
   hid_t locId,
   hsize_t *packetCount);

// H5PTis_valid
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTis_valid(
   hid_t locId);

// H5PTis_varlen
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTis_varlen(
   hid_t locId);

// H5PTopen
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" hid_t _cdecl H5PTopen(
   hid_t locId,
   [MarshalAs(UnmanagedType::LPStr)]String^ dataSetName);

// H5PTread_packets
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTread_packets(
   hid_t locId,
   hsize_t offset,
   size_t packetCount,
   void* data);

// H5PTset_index
[DllImport("hdf5_hldll.dll",
           CharSet=CharSet::Auto,
           CallingConvention=CallingConvention::StdCall)]
extern "C" herr_t _cdecl H5PTset_index(
   hid_t locId,
   hsize_t index);

#pragma endregion Externals

namespace HDF5DotNet
{
    generic <class Type>
    void H5PT::Append(
        H5PacketTableId^ locationId,
        size_t packetCount,
        H5Array<Type>^ data)
    {
        pin_ptr<Type> pinnedDataPtr = data->getDataAddress();
        void* voidPtr = pinnedDataPtr;

        // Write using the pinned array
        herr_t status = H5PTappend(
            locationId->Id, 
            packetCount,
            voidPtr);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.Append: \n"
                "Failed to append data to packet table {0:x} with status {1}\n",
                locationId->Id,
                status);

            throw gcnew H5PTappendException(message, status);
        }
    }

    void H5PT::Close(H5PacketTableId^ id)	
    {
        herr_t status = H5PTclose(id->Id);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.Close: \n"
                "  Failed to close packet table {0:x} with status {1}\n",
                id->Id, status);

            throw gcnew H5PTcloseException(message, status);
        }
    }

    H5PacketTableId^ H5PT::CreateFixedLength(
        H5LocId^ groupOrFileId, 
        String^ dataSetName,
        H5DataTypeId^ dataType,
        hsize_t chunkSize,
        int compressionLevel)
    {
        hid_t status = H5PTcreate_fl(
            groupOrFileId->Id, 
            dataSetName,
            dataType->Id,
            chunkSize,
            compressionLevel);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.CreateFixedLength: \n"
                "  Failed to create packet table with status = {0}\n"
                "    groupOrFileId = {1:x}\n"
                "    datasetName = {2}\n"
                "    dataType = {3:x}\n"
                "    chunkSize = {4}\n"
                "    compressionLevel = {5}\n",
                status,
                groupOrFileId->Id,
                dataSetName,
                dataType->Id, 
                chunkSize,
                compressionLevel);
            throw gcnew H5PTcreateFixedLengthException(message, status);
        }

        return gcnew H5PacketTableId(status);
    }

    void H5PT::CreateIndex(H5PacketTableId^ locationId)
    {
        herr_t status = H5PTcreate_index(locationId->Id);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.CreateIndex: \n"
                "  Failed to create index on packet table ID {0:x} with status {1}\n",
                locationId->Id,
                status);

            throw gcnew H5PTcreateIndexException(message, status);
        }
    }

    H5PacketTableId^ H5PT::CreateVariableLength(
        H5LocId^ groupOrFileId, 
        String^ dataSetName,
        hsize_t chunkSize)
    {
        hid_t status = H5PTcreate_vl(
            groupOrFileId->Id, 
            dataSetName,
            chunkSize);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.CreateVariableLength: \n"
                "  Failed to create packet table with status = {0}\n"
                "    groupOrFileId = {1:x}\n"
                "    datasetName = {2}\n"
                "    chunkSize = {4}\n",
                status,
                groupOrFileId->Id,
                dataSetName,
                chunkSize);
            throw gcnew H5PTcreateVariableLengthException(message, status);
        }

        return gcnew H5PacketTableId(status);
    }

    hsize_t H5PT::GetIndex(H5PacketTableId^ locationId)
    {
        hsize_t index;

        herr_t status = H5PTget_index(
            locationId->Id,
            &index);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.GetIndex: \n"
                "  Failed to get index for id {0:x} with status {1}\n",
                locationId->Id,
                status);

            throw gcnew H5PTgetIndexException(message, status);
        }

        return index;
    }

    generic <class Type>
    void H5PT::GetNext(
        H5PacketTableId^ locationId,
        size_t packetCount,
        H5Array<Type>^ data)
    {
        pin_ptr<Type> pinnedDataPtr = data->getDataAddress();
        void* voidPtr = pinnedDataPtr;

        // Read using the pinned array
        herr_t status = H5PTget_next(
            locationId->Id, 
            packetCount, 
            voidPtr);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.GetNext: \n"
                "Failed to read data from packet table {0:x} with status {1}\n",
                locationId->Id,
                status);

            throw gcnew H5PTgetNextException(message, status);
        }
    }

    hsize_t H5PT::GetNumPackets(H5PacketTableId^ locationId)
    {
        hsize_t packetCount;

        herr_t status = H5PTget_num_packets(
            locationId->Id,
            &packetCount);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.GetNumPackets: \n"
                "  Failed to get information for id {0:x} with status {1}\n",
                locationId->Id,
                status);

            throw gcnew H5PTgetNumPacketsException(message, status);
        }

        return packetCount;
    }

    bool H5PT::IsValid(H5PacketTableId^ locationId)
    {
        herr_t status = H5PTis_valid(locationId->Id);

        /*if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.IsValid: \n"
                "  Failed to get information for id {0:x} with status {1}\n",
                locationId->Id,
                status);
            throw gcnew H5PTIsValidException(message, status);
        }*/

        return (status >= 0);
    }

    bool H5PT::IsVariableLength(H5PacketTableId^ locationId)
    {
        herr_t status = H5PTis_varlen(locationId->Id);

        /*if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.IsVariableLength: \n"
                "  Failed to get information for id {0:x} with status {1}\n",
                locationId->Id,
                status);
            throw gcnew H5PTIsVariableLengthException(message, status);
        }*/

        return (status > 0);
    }

    H5PacketTableId^ H5PT::Open(H5LocId^ groupOrFileId, String^ dataSetName)
    {
        herr_t status = H5PTopen(
            groupOrFileId->Id,
            dataSetName);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.Open: \n"
                "Failed to open packet table {0} from groupOrFileId {1:x} with status {2}\n",
                dataSetName, groupOrFileId->Id, status);

            throw gcnew H5PTopenException(message, status);
        }

        return gcnew H5PacketTableId(status);
    }

    generic <class Type>
    void H5PT::Read(
        H5PacketTableId^ locationId,
        hsize_t offset,
        size_t packetCount,
        H5Array<Type>^ data)
    {
        pin_ptr<Type> pinnedDataPtr = data->getDataAddress();
        void* voidPtr = pinnedDataPtr;

        // Read using the pinned array
        herr_t status = H5PTread_packets(
            locationId->Id, 
            offset,
            packetCount, 
            voidPtr);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.Read: \n"
                "Failed to read data from packet table {0:x} at offset {1:x} with status {2}\n",
                locationId->Id,
                offset,
                status);

            throw gcnew H5PTreadException(message, status);
        }
    }

    void H5PT::SetIndex(
        H5PacketTableId^ locationId,
        hsize_t index)
    {
        herr_t status = H5PTset_index(
            locationId->Id,
            index);

        if (status < 0)
        {
            String^ message = String::Format(
                "H5PT.SetIndex: \n"
                "  Failed to set packet table index {0} from packet table ID {1:x} with status {2}\n",
                index,
                locationId->Id,
                status);

            throw gcnew H5PTsetIndexException(message, status);
        }
    }
}