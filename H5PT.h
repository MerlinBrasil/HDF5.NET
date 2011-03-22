/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by The HDF Group.                                               *
 * Copyright by Jesse Lai.                                                   *
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
using namespace System;
#include "stdafx.h"

#include "H5Common.h"
#include "H5PTpublicNET.h" // modified version of H5PTpublic.h
#include "HDFException.h"
#include "HDFExceptionSubclasses.h"

#define _HDF5USEDLL_
#define HDF5_DOT_NET

namespace HDF5DotNet
{
    /// <summary>
    /// The H5PT contains static member function for each of the supported
    /// H5PT calls of the HDF5 library.
    /// </summary>
    public ref class H5PT
    {
    public:
        /// <summary>
        /// Appends packets to the end of a packet table.
        /// </summary>
        /// <remarks>
        /// Append writes recordCount packets to the end of a packet table
        /// specified by locationId. The input data is a buffer containing the data to be written.
        /// For a packet table holding fixed-length packets, this data should be in the
        /// packet table's datatype. For a variable-length packet table, the data should
        /// be in the form of hvl_t structs.
        /// </remarks>
        /// <typeparam name="Type">The type of data to append.</typeparam>
        /// <param name="locationId">
        /// Id of packet table.
        /// </param>
        /// <param name="packetCount">
        /// The number of packets to be appended. 
        /// </param>
        /// <param name="data>
        /// Array with data to be written to the file.
        /// </param>
        /// <exception cref="H5PTAppendException">
        /// Throws H5PTAppendException if append fails
        /// </exception>
        generic <class Type>
        static void Append(
            H5PacketTableId^ locationId,
            size_t packetCount,
            H5Array<Type>^ data);

        /// <summary>
        /// Close a packet table.
        /// </summary>
        /// <param name="id">
        /// Id of packet table to close.
        /// </param>
        /// <exception cref="H5PTCloseException"> 
        /// Throws H5PTCloseException if close fails
        /// </exception>
        static void Close(H5PacketTableId^ id);

        /// <summary>
        /// Creates a new dataset and links it into the file.
        /// </summary>
        /// <param name="groupOrFileId">
        /// Location identifier
        /// </param>
        /// <param name="dataSetName">
        /// Dataset name
        /// </param>
        /// <param name="dataTypeId">
        /// Identifier of the datatype.
        /// </param>
        /// <param name="chunkSize">
        /// The packet table uses HDF5 chunked storage to allow it
        /// to grow. This value allows the user to set the size of
        /// a chunk. The chunk size affects performance. 
        /// </param>
        /// <param name="compressionLevel">
        /// A value of 0 through 9. Level 0 is faster but offers the least
        /// compression; level 9 is slower but offers maximum compression.
        // A setting of -1 indicates that no compression is desired.
        /// </param>
        /// <returns>
        /// A H5PacketTableId associated with the created packet table.
        /// </returns>
        /// <exception cref="H5PTCreateFixedLengthException">
        /// Throws H5PTCreateFixedLengthException on failure.
        /// </exception>
        static H5PacketTableId^ CreateFixedLength(
            H5LocId^ groupOrFileId, 
            String^ dataSetName,
            H5DataTypeId^ dataTypeId,
            hsize_t chunkSize,
            int compressionLevel);

        /// <summary>
        /// Resets a packet table's index to the first packet.
        /// Each packet table keeps an index of the "current" packet so that GetNext
        /// can iterate through the packets in order. The method initializes a packet
        /// table's index, and should be called before using GetNext. The index must
        /// be initialized every time a packet table is created or opened. This
        /// information is lost when the packet table is closed.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <exception cref="H5PTCreateIndexException">
        /// Throws H5PTCreateIndexException on failure.
        /// </exception>
        static void CreateIndex(H5PacketTableId^ locationId);

        /// <summary>
        /// Creates a new dataset and links it into the file.
        /// </summary>
        /// <param name="groupOrFileId">
        /// Location identifier
        /// </param>
        /// <param name="dataSetName">
        /// Dataset name
        /// </param>
        /// <param name="chunkSize">
        /// The packet table uses HDF5 chunked storage to allow it
        /// to grow. This value allows the user to set the size of
        /// a chunk. The chunk size affects performance. 
        /// </param>
        /// <returns>
        /// A H5PacketTableId associated with the created packet table.
        /// </returns>
        /// <exception cref="H5PTCreateVariableLengthException">
        /// Throws H5PTCreateVariableLengthException on failure.
        /// </exception>
        static H5PacketTableId^ CreateVariableLength(
            H5LocId^ groupOrFileId, 
            String^ dataSetName,
            hsize_t chunkSize);

        /// <summary>
        /// Gets a packet table's index.
        /// Each packet table keeps an index of the "current" packet so that GetNext
        /// can iterate through the packets in order. The method sets this index to
        /// point to a user-specified packet. The packets are zero-indexed.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <returns>
        /// The index of the "current" packet.
        /// </returns>
        /// <exception cref="H5PTGetIndexException">
        /// Throws H5PTGetIndexException on failure.
        /// </exception>
        static hsize_t GetIndex(H5PacketTableId^ locationId);

        /// <summary>
        /// Reads packets from a packet table starting at the current index.
        /// </summary>
        /// <remarks>
        /// GetNext reads packetCount packets starting with the "current index"
        /// from a packet table specified by locationId. The packet table's index is set
        /// and reset with SetIndex and CreateIndex. The variable
        /// data is a buffer containing the data to be read.
        /// For a packet table holding variable-length records, the data returned in
        /// the buffer will be in form of a hvl_t struct containing the length of the
        /// data and a pointer to it in memory. 
        /// </remarks>
        /// <param name="locationId">
        /// Id of packet table.
        /// </param>
        /// <param name="packetCount">
        /// The number of packets to be appended. 
        /// </param>
        /// <param name="data>
        /// Array with data to be written to the file.
        /// </param>
        /// <exception cref="H5PTGetNextException"> 
        /// Throws H5PTGetNextException if close fails
        /// </exception>
        generic <class Type>
        static void GetNext(
            H5PacketTableId^ locationId,
            size_t packetCount,
            H5Array<Type>^ data);

        /// <summary>
        /// Gets the number of packets in a packet table.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <returns>
        /// The number of packets in packet table. 
        /// </returns>
        /// <exception cref="H5PTGetNumPacketsException">
        /// Throws H5PTGetNumPacketsException on failure.
        /// </exception>
        static hsize_t GetNumPackets(H5PacketTableId^ locationId);

        /// <summary>
        /// Determines whether an indentifier points to a packet table.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <returns>
        /// True if the identifier points to an open packet table; false otherwise.
        /// </returns>
        /// <exception cref="H5PTIsValidException">
        /// Throws H5PTIsValidException on failure.
        /// </exception>
        static bool IsValid(H5PacketTableId^ locationId);

        /// <summary>
        /// Determines whether the packet table contains variable length
        /// data.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <returns>
        /// True if the identifier points to a packet table with variable
        /// length data; false otherwise.
        /// </returns>
        /// <exception cref="H5PTIsVariableLengthException">
        /// Throws H5PTIsVariableLengthException on failure.
        /// </exception>
        static bool IsVariableLength(H5PacketTableId^ locationId);

        /// <summary>
        /// Opens an existing packet table.
        /// </summary>
        /// <param name="groupOrFileId">
        /// Identifier of the file or
        /// group within which the dataset to be accessed will be found. 
        /// </param>
        /// <param name="dataSetName">
        /// The name of the dataset to access.
        /// </param>
        /// <returns>
        /// A H5PacketTableId associated with the created packet table.
        /// </returns>
        /// <exception cref="H5PTOpenException">
        /// Throws H5PTOpenException on failure.
        /// </exception>
        /// <remarks>
        /// Opens an existing packet table for access in the file or 
        /// group specified in groupOrFileId. The dataSetName is a dataset name and 
        /// is used to identify the packet table in the file.
        /// </remarks>
        static H5PacketTableId^ Open(
            H5LocId^ groupOrFileId, 
            String^ dataSetName);

        /// <summary>
        /// Reads a number of packets from a packet table.
        /// </summary>
        /// <remarks>
        /// Reads packetCount packets starting at packet number 
        /// offset from a packet table specified by locationId. The variable
        /// data is a buffer containing the data to be read.
        /// For a packet table holding variable-length records, the data
        /// returned in the buffer will be in form of hvl_t structs, each
        /// containing the length of the data and a pointer to it in memory.
        /// </remarks>
        /// <param name="locationId">
        /// Id of packet table.
        /// </param>
        /// <param name="packetCount">
        /// The number of packets to be appended. 
        /// </param>
        /// <param name="data>
        /// Array with data to be written to the file.
        /// </param>
        /// <exception cref="H5PTReadException"> 
        /// Throws H5PTReadException if close fails
        /// </exception>
        generic <class Type>
        static void Read(
            H5PacketTableId^ locationId,
            hsize_t offset,
            size_t packetCount,
            H5Array<Type>^ data);

        /// <summary>
        /// Sets a packet table's index.
        /// Each packet table keeps an index of the "current" packet so that GetNext
        /// can iterate through the packets in order. The method sets this index to
        /// point to a user-specified packet. The packets are zero-indexed.
        /// </summary>
        /// <param name="locationId">
        /// The packet table identifier
        /// </param>
        /// <param name="index">
        /// The index of the "current" packet.
        /// </param>
        /// <exception cref="H5PTSetIndexException">
        /// Throws H5PTSetIndexException on failure.
        /// </exception>
        static void SetIndex(
            H5PacketTableId^ locationId,
            hsize_t index);
    private:
        /// <summary>
        /// Disallow the creation of instances of this class.
        /// </summary>
        H5PT() { };
    };
}