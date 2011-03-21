using System;
using System.Collections.Generic;
using System.Text;
using HDF5DotNet;

namespace h5groups
{
    using hsize_t = System.UInt64;
    class Program
    {
        static int nerrors = 0;
        const string FILE_NAME = ("Group.h5");
        const int RANK = 2;
        static void test_group_basics()
        {
            try
            {
                Console.Write("Testing group basics");

                // Create the file.
                H5FileId fileId = H5F.create(FILE_NAME, H5F.CreateMode.ACC_TRUNC);

                // Create a group.
                H5GroupId groupId = H5G.create(fileId, "/fromRoot", 0);

                // Create a dataspace for common use.
                hsize_t[] dims = { 1000, 20 };
                H5DataSpaceId dspace = H5S.create_simple(RANK, dims);

                // Create a dataset using file as location with absolute path name.
                H5DataSetId dset1Id = H5D.create(fileId, "/fromRoot/intArray", H5T.H5Type.NATIVE_INT, dspace);

                // Create a dataset using group as location with absolute path name.
                H5DataSetId dset2Id = H5D.create(groupId, "/fromRoot/shortArray", H5T.H5Type.NATIVE_SHORT, dspace);

                // Create a dataset using group as location with relative path name.
                H5DataSetId dset3Id = H5D.create(groupId, "notfromRoot", H5T.H5Type.NATIVE_UCHAR, dspace);
                
                ObjectInfo info = H5G.getObjectInfo(fileId, "/fromRoot/intArray", true);
                if (info.nHardLinks != 1)
                    Console.WriteLine("\ntest_group_basics: number of hardlinks for /fromRoot/intArray should be = {0}", info.nHardLinks);
                if (info.objectType != H5GType.DATASET)
                    Console.WriteLine("\ntest_group_basics: Object should be a dataset");
                
                // Close objects and files.
                H5D.close(dset1Id);
                H5D.close(dset2Id);
                H5D.close(dset3Id);
                H5S.close(dspace);
                H5G.close(groupId);

                // Check various number of objects.
                H5GroupId rootId = H5G.open(fileId, "/");
                hsize_t num_objs = H5G.getNumObjects(rootId);
                if (num_objs != 1)
                {
                    Console.WriteLine("\ntest_group_basics: incorrect num_objs = {0} for root group\n", num_objs);
                    nerrors++;
                }

                groupId = H5G.open(fileId, "fromRoot");
                num_objs = H5G.getNumObjects(groupId);
                if (num_objs != 3)
                {
                    Console.WriteLine("\ntest_group_basics: incorrect num_objs = {0} for group \"fromRoot\"\n", num_objs);
                    nerrors++;
                }

                H5G.close(rootId);
                H5G.close(groupId);
                H5F.close(fileId);

                if (nerrors == 0)
                    Console.WriteLine("\t\t\tPASSED");
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
        } // test_group_basics

        /*
         * Operator function.
         */
        static int file_info(H5GroupId id, string objectName, Object param)
        {
            Console.WriteLine("The object name is {0}", objectName);
            return 0;
        }

        /*
         * test_group_iterate -- tests that group iterating works properly.
         *      - opens the file created in the test_group_basics
         *      - creates more groups and datasets
         *      - iterates through root group and each sub group priting out name of each object
         */
        static void test_group_iterate()
        {
            try
            {
                Console.Write("Testing group iteration");

                // Open the file.
                H5FileId fileId = H5F.open(FILE_NAME, H5F.OpenMode.ACC_RDWR);

                // Create a group in the file.
                H5GroupId groupId = H5G.create(fileId, "/Data", 0);

                // Open first dataset.
                H5DataSetId dset1Id = H5D.open(fileId, "/fromRoot/intArray");

                // Get dataspace of this dataset.
                H5DataSpaceId dspace = H5D.getSpace(dset1Id);

                // Create a dataset in the group using absolute name.
                H5DataSetId dataset = H5D.create(fileId, "/Data/IntData", H5T.H5Type.NATIVE_INT, dspace);

                // Close the first dataset.
                H5S.close(dspace);
                H5D.close(dataset);

                // Create the second dataset.

                hsize_t[] dims = { 500, 20 };
                dspace = H5S.create_simple(RANK, dims);
                dataset = H5D.create(groupId, "/Data/FloatData", H5T.H5Type.NATIVE_FLOAT, dspace);

                // Close objects and file.
                H5D.close(dataset);
                H5G.close(groupId);
                H5F.close(fileId);

                // Now reopen the file and group in the file.
                fileId = H5F.open(FILE_NAME, H5F.OpenMode.ACC_RDWR);
                groupId = H5G.open(fileId, "/Data");
                try
                {
                    // Access "IntData" dataset in the group.
                    dataset = H5D.open(groupId, "IntData");
                }
                catch (H5DopenException)
                {
                    Console.WriteLine("\ntest_group_iterate: Dataset 'IntData' is not found.");
                    nerrors++;
                }

                // Create a dataset in the root group.
                dataset = H5D.create(fileId, "/SingleDataset", H5T.H5Type.NATIVE_INT, dspace);

                // Various checks on number of objects
                groupId = H5G.open(fileId, "/");
                hsize_t num_objs = H5G.getNumObjects(groupId);
                if (num_objs != 3)
                    Console.WriteLine("\ntest_group_iterate: / should have 3 objects: /fromRoot, /Data, and /SingleDataset, but is {0}", num_objs);
                H5G.close(groupId);

                groupId = H5G.open(fileId, "/fromRoot");
                num_objs = H5G.getNumObjects(groupId);
                if (num_objs != 3)
                    Console.WriteLine("\ntest_group_iterate: /fromRoot should have 3 objects: intArray, shortArray, notfromRoot, but is {0}", num_objs);

                // Use iterator to see the names of the objects in the root group and sub groups.
                H5GIterateDelegate myDelegate;
                myDelegate = file_info;
                int x = 1;

                Console.WriteLine();
                Console.WriteLine("Group iterating:");

                int index = H5G.iterate(fileId, "/", myDelegate, x, 0);
                index = H5G.iterate(fileId, "/fromRoot", myDelegate, x, 0);
                index = H5G.iterate(fileId, "/Data", myDelegate, x, 0);

                // Close objects and file.
                H5D.close(dataset);
                H5G.close(groupId);
                H5S.close(dspace);
                H5F.close(fileId);

                Console.WriteLine("\t\t\tPASSED");
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
        }

        static void Main(string[] args)
        {
            test_group_basics();
            test_group_iterate();

            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test(s) failed: {0}", nerrors, "occurred!");
            else
                Console.WriteLine("All group tests passed.");
            Console.WriteLine();
        }
    }
}
