/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright by Martin Galpin (66laps Limited).                            *
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
using System;
using System.Collections.Generic;   
using System.Linq;
using System.Text;
using HDF5DotNet;
using System.Runtime.InteropServices;
using System.IO;

namespace h5pt
{
    class Program
    {
        static int nerrors = 0;
        const string FILENAME = ("test_h5pt.h5");
        
        static void test_h5pt_fl()
        {
            Console.WriteLine("Test H5PT with a fixed length basic table");
            
            // Delete file if it exists.
            File.Delete(FILENAME);
            
            // Create the file.
            H5FileId fileId = H5F.create(FILENAME, H5F.CreateMode.ACC_TRUNC);
            
            // Create a packet table.     
            H5PacketTableId tableId = H5PT.CreateFixedLength(fileId, "Packets", new H5DataTypeId(H5T.H5Type.NATIVE_INT), 100, 0);
            
            // Check the new packet table is valid.
            if(!H5PT.IsValid(tableId))
            {
                nerrors++;
                return;
            }
            
            // Append data.
            const int numPackets = 1000;
            for(int i = 0; i < numPackets; i++)
            {
                try
                {
                    H5PT.Append(tableId, 1, new H5Array<int>(new int[] { i }));
                }
                catch(H5PTappendException exception)
                {
                    Console.WriteLine(exception.Message);
                    nerrors++;
                }
            }
            
            // Count the number of packets.
            if(H5PT.GetNumPackets(tableId) != numPackets)
            {
                Console.WriteLine("Incorrect number of packets.");
                nerrors++;
            }
            
            // Create a new table index.
            try
            {
                H5PT.CreateIndex(tableId);
            }
            catch(H5PTcreateIndexException exception)
            {
                Console.WriteLine(exception.Message);
                nerrors++;
            }
            
            // Read each packet.
            int packetsRead = 0;
            try
            {
                while(true)
                {
                    int[] outData = new int[] { 0 };
                    H5PT.GetNext(tableId, 1, new H5Array<int>(outData));
                    packetsRead++;
                }
            }
            catch(H5PTgetNextException)
            {
                // FIXME Reading until EOF (on exception) is not very nice.                
            }
            
            // Check the correct number of packets were read.
            if(packetsRead != numPackets)
            {
                Console.WriteLine("Read incorrect number of packets.");
                nerrors++;
            }
            
            // Reset index to beginning.
            H5PT.SetIndex(tableId, 0);
            
            // Read all data in a single call.
            int[] allData = new int[numPackets];
            try
            {
                H5PT.Read(tableId, 0, numPackets, new H5Array<int>(allData));
            }
            catch(H5PTreadException exception)
            {
                Console.WriteLine(exception.Message);
                nerrors++;
            }
            
            H5PT.Close(tableId);
            H5F.close(fileId);
        }

        struct DataPacket
        {
            public long X;
            public double Y;
        };
        
        static void test_h5pt_compound()
        {   
            Console.WriteLine("Test H5PT with compound types");
            
            // Delete file if it exists.
            File.Delete(FILENAME);
            
            // Create the compound datatype for DataPacket.
            H5DataTypeId compoundType = H5T.create(H5T.CreateClass.COMPOUND, (uint)Marshal.SizeOf(typeof(DataPacket)));
            H5T.insert(compoundType, "X", 0, H5T.H5Type.STD_U64LE);
            H5T.insert(compoundType, "Y", sizeof(long), H5T.H5Type.IEEE_F64LE);

            // Create the file.
            H5FileId fileId = H5F.create(FILENAME, H5F.CreateMode.ACC_TRUNC);

            // Create a packet table.     
            H5PacketTableId tableId = H5PT.CreateFixedLength(fileId, "Compound Packets", compoundType, 100, 5);

            // Check the new packet table is valid.
            if (!H5PT.IsValid(tableId))
            {
                nerrors++;
                return;
            }

            // Append data.
            const int numPackets = 10000;
            for (int i = 0; i < numPackets; i++)
            {
                try
                {
                    DataPacket pack = new DataPacket{ X = i, Y = (double) i };
                    H5PT.Append(tableId, 1, new H5Array<DataPacket>(new DataPacket[] { pack }));
                }
                catch (H5PTappendException exception)
                {
                    Console.WriteLine(exception.Message);
                    nerrors++;
                }
            }

            // Count the number of packets.
            if (H5PT.GetNumPackets(tableId) != numPackets)
            {
                Console.WriteLine("Incorrect number of packets.");
                nerrors++;
            }

            // Create a new table index.
            try
            {
                H5PT.CreateIndex(tableId);
            }
            catch (H5PTcreateIndexException exception)
            {
                Console.WriteLine(exception.Message);
                nerrors++;
            }

            H5PT.Close(tableId);
            H5F.close(fileId);
        }
        
        static void Main(string[] args)
        {
            test_h5pt_fl();
            test_h5pt_compound();
            
            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test(s) failed: {0}", nerrors, "occurred!");
            else
                Console.WriteLine("All group tests passed.");
            Console.WriteLine();
        }
    }
}
