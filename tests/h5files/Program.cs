using System;
using System.Collections.Generic;
using System.Text;
using HDF5DotNet;
using System.IO;

namespace h5files
{
    using hsize_t = System.UInt64;
    class Program
    {
        static int nerrors = 0;
        const string FILE1 = ("test_files.h5");
        const int RANK = 1; // one-dimension
        static void test_file_create()
        {
            try
            {
                Console.Write("Testing File Creation I/O");

                // First ensure the file does not exist
                File.Delete(FILE1);
                
                // Test create with various sequences of H5F_ACC_EXCL and H5F_ACC_TRUNC flags,
                H5FileId fileId = H5F.create(FILE1, H5F.CreateMode.ACC_EXCL);

                // Try to create the same file with H5F_ACC_TRUNC. This should fail 
                // because fid1 is the same file and is currently open.
                try
                {
                    H5FileId fid1 = H5F.create(FILE1, H5F.CreateMode.ACC_TRUNC);

                    // should fail, but didn't, print out the error message.
                    Console.WriteLine("\ntest_file_create: Attempting to truncate an opened file.");
                    nerrors++;
                }
                catch (H5FcreateException) {} // does nothing, it should fail

                H5F.close(fileId);

                // Try again with H5F_ACC_EXCL. This should fail because the file already
                // exists from the previous steps.
                try
                {
                    H5FileId fid2 = H5F.create(FILE1, H5F.CreateMode.ACC_EXCL);

                    // should fail, but didn't, print out the error message.
                    Console.WriteLine("\ntest_file_create: Attempting to truncate an existent file.");
                    nerrors++;
                }
                catch (H5FcreateException) {} // does nothing, it should fail

                // Test create with H5F_ACC_TRUNC. This will truncate the existing file.
                fileId = H5F.create(FILE1, H5F.CreateMode.ACC_TRUNC);

                // Try to truncate first file again. This should fail because fid1 is the
                // same file and is currently open.
                try
                {
                    H5FileId fid3 = H5F.create(FILE1, H5F.CreateMode.ACC_TRUNC);

                    // should fail, but didn't, print out the error message.
                    Console.WriteLine("\ntest_file_create: Attempting to truncate an opened file.");
                    nerrors++;
                }
                catch (H5FcreateException) {} // does nothing, it should fail

                // Try with H5F_ACC_EXCL. This should fail too because the file already exists.
                try
                {
                    H5FileId fid4 = H5F.create(FILE1, H5F.CreateMode.ACC_EXCL);

                    // should fail, but didn't, print out the error message.
                    Console.WriteLine("\ntest_file_create: Attempting to truncate an existent file.");
                    nerrors++;
                }
                catch (H5FcreateException) {} // does nothing, it should fail

   
                Console.WriteLine("\t\tPASSED");
            } // end try block
            catch (HDFException anyHDF5E)
            {
                Console.WriteLine(anyHDF5E.Message);
                nerrors++;
            }
            catch (System.Exception sysE)
            {
                Console.WriteLine(sysE.TargetSite);
                Console.WriteLine(sysE.Message);
            }
        } // test_file_create

        const string FILE2 = ("File2.h5");
        const string GROUP_NAME = ("/fromRoot");
        const string DSET1_NAME = ("IntArray");
        const string DSET2_NAME = ("ShortArray");
        static void test_file_open()
        {
            try
            {
                // Output message about test being performed.
                Console.Write("Testing File Opening I/O");

                // First ensure the file does not exist
                File.Delete(FILE2);

                // Try opening a non-existent file.  This should fail.
                try
                {
                    H5FileId non_exist_file = H5F.open(FILE2, H5F.OpenMode.ACC_RDWR);

                    // should fail, but didn't, print out the error message.
                    Console.WriteLine("\ntest_file_open: Attempting to open a non-existent file.");
                    nerrors++;
                }
                catch (H5FopenException) {} // does nothing, it should fail

                // Open the file.
                H5FileId fileId = H5F.open(FILE1, H5F.OpenMode.ACC_RDWR);

                // Create dataspace for the dataset in the file.
                hsize_t[] dims = {20 };
                H5DataSpaceId dspace = H5S.create_simple(RANK, dims);

                // Create a group.
                H5GroupId groupId = H5G.create(fileId, GROUP_NAME, 0);

                // Create a dataset using file as location.
                H5DataSetId dset1Id = H5D.create(fileId, DSET1_NAME, H5T.H5Type.NATIVE_INT, dspace);

                // Create a dataset using group as location.
                H5DataSetId dset2Id = H5D.create(groupId, DSET2_NAME, H5T.H5Type.NATIVE_SHORT, dspace);

                // Close objects and files.
                H5D.close(dset1Id);
                H5D.close(dset2Id);
                H5S.close(dspace);
                H5G.close(groupId);
                H5F.close(fileId);

                Console.WriteLine("\t\tPASSED");
            }
            catch (HDFException anyHDF5E)
            {
                Console.WriteLine(anyHDF5E.Message);
                nerrors++;
            }
            catch (System.Exception sysE)
            {
                Console.WriteLine(sysE.TargetSite);
                Console.WriteLine(sysE.Message);
                nerrors++;
            }
        } // test_file_open

        static void Main(string[] args)
        {
            // Suppress error printing from the C library.
            H5E.suppressPrinting();

            test_file_create();
            test_file_open();

            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test(s) failed: {0}", nerrors, "occurred!");
            else
                Console.WriteLine("All file tests passed.");
            Console.WriteLine();
        }

    }
}
