using System;
using System.Collections.Generic;
using System.Text;
using HDF5DotNet;

namespace dsets
{
    using hsize_t = System.UInt64;
    using uchar = System.Char;
    class Program
    {
        const int DSET_DIM1 = 1;
        const int DSET_DIM2 = 2;
        static int nerrors = 0;

        static void test_create(H5FileId fileId)
        {
            try
            {
                Console.Write("Testing create, open, and close datasets");

                const string DSET_DEFAULT_NAME = ("default");
                const int RANK = 2; // one-dimension

                // Create the data space.
                hsize_t[] dims = { 256, 512 };
                H5DataSpaceId space = H5S.create_simple(RANK, dims);

                // Create a small data space for compact dataset.
                hsize_t[] small_dims = { 16, 8 };
                H5DataSpaceId small_space = H5S.create_simple(RANK, small_dims);

                // Create a dataset using the default dataset creation properties.
                H5DataSetId dataset = H5D.create(fileId, DSET_DEFAULT_NAME, H5T.H5Type.NATIVE_DOUBLE, space);

                // Close the dataset.
                H5D.close(dataset);

                // Try creating a dataset that already exists.  This should fail since a
                // dataset can only be created once.
                try
                {
                    dataset = H5D.create(fileId, DSET_DEFAULT_NAME, H5T.H5Type.NATIVE_DOUBLE, space);

                    // should fail, but didn't, print an error message.
                    Console.WriteLine("\ntest_create: Attempting to create an existing dataset.");
                    nerrors++;
                }
                catch (HDFException) {} // does nothing, it should fail

                // Open the dataset we created above and then close it.  This is how
                // existing datasets are accessed.
                dataset = H5D.open(fileId, DSET_DEFAULT_NAME);
                H5D.close(dataset);

                // Try opening a non-existent dataset. This should fail since new datasets
                // cannot be created with this function.
                try
                {
                    dataset = H5D.open(fileId, "does_not_exist");

                    // should fail, but didn't, print an error message.
                    Console.WriteLine("\ntest_create: Opened a non-existent dataset.");
                    nerrors++;
                }
                catch (HDFException) {} // does nothing, it should fail

                Console.WriteLine("\tPASSED");
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
                nerrors++;
            }
        } // test_create

        static void test_onedim_array(H5FileId fileId)
        {
            try
            {
                Console.Write("Testing write/read one-dimensional array");

                const string DSET_NAME = ("One-dim IntArray");
                const int NX = 5;   // data set dimension
                const int RANK = 1; // one-dimension

                // Data  and output buffer initialization.
                int i;
                int[] data = new int[NX];
                for (i = 0; i < NX; i++)
                    data[i] = i;

                // Describe the size of the array and create the data space for fixed
                // size dataset.
                ulong[] dims = { NX };
                H5DataSpaceId dspaceId = H5S.create_simple(RANK, dims);

                // Define datatype for the data in the file.
                // We will store little endian INT numbers.
                H5DataTypeId dtypeId = H5T.copy(H5T.H5Type.STD_I32LE);

                // Create the data set DATASETNAME.
                H5DataSetId dsetId = H5D.create(fileId, DSET_NAME, dtypeId, dspaceId);

                // Write the one-dimensional data set array
                H5D.write(dsetId, new H5DataTypeId(H5T.H5Type.STD_I32LE), new H5Array<int>(data));

                int[] outdata = new int[NX];
                for (i = 0; i < NX; i++) outdata[i] = 0;

                // Read data back.
                H5D.read(dsetId, new H5DataTypeId(H5T.H5Type.STD_I32LE), new H5Array<int>(outdata));

                // Compare against input buffer to verify.
                for (i = 0; i < NX; i++)
                {
                    if (outdata[i] != data[i])
                    {
                        Console.WriteLine("\ntest_onedim_array: read value differs from input: read {0} - input {1}",
                            outdata[i], data[i]);
                        nerrors++;
                    }
                }

                // Close all objects and file.
                H5D.close(dsetId);
                H5T.close(dtypeId);
                H5S.close(dspaceId);

                Console.WriteLine("\tPASSED");
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
        } // test_onedim_array

        static void test_twodims_array()
        {
            try
            {
                Console.Write("Testing write/read two-dimensional array");

                const string FILE_NAME = ("SDStwodim.h5");
                const string DSET_NAME = ("Two-dim IntArray");
                const int NX = 5;   // data set dimension
                const int NY = 2;
                const int RANK = 2; // two-dimension

                // Data and input buffer initialization.
                int i, j;
                int[,] data = new int[NX, NY];
                for (i = 0; i < NX; i++)
                    for (j = 0; j < NY; j++)
                        data[i, j] = i + j;

                // Create a new file using H5F_ACC_TRUNC access,
                // default file creation properties, and default file
                // access properties.
                H5FileId fileId = H5F.create(FILE_NAME, H5F.CreateMode.ACC_TRUNC);

                // Describe the size of the array and create the data space for fixed
                // size dataset.
                ulong[] dims = new ulong[RANK];
                dims[0] = NX;
                dims[1] = NY;
                H5DataSpaceId dspaceId = H5S.create_simple(RANK, dims);

                // Define datatype for the data in the file.
                H5DataTypeId dtypeId = H5T.copy(H5T.H5Type.NATIVE_INT);

                // Create the data set DATASETNAME.
                H5DataSetId dsetId = H5D.create(fileId, DSET_NAME, dtypeId, dspaceId);

                // Write the one-dimensional data set array
                H5D.write(dsetId, new H5DataTypeId(H5T.H5Type.NATIVE_INT), new H5Array<int>(data));

                // Close dataset and file.
                H5D.close(dsetId);
                H5F.close(fileId);

                // Open the file again in read only mode.
                fileId = H5F.open(FILE_NAME, H5F.OpenMode.ACC_RDONLY);

                // Open the dataset using its name.
                dsetId = H5D.open(fileId, DSET_NAME);

                int[,] outdata = new int[NX, NY];
                for (i = 0; i < NX; i++)
                    for (j = 0; j < NY; j++)
                        outdata[i, j] = 0;

                // Read data back.
                H5D.read(dsetId, new H5DataTypeId(H5T.H5Type.NATIVE_INT), new H5Array<int>(outdata));

                // Compare against input buffer to verify.
                for (i = 0; i < NX; i++)
                    for (j = 0; j < NY; j++)
                    {
                        if (outdata[i, j] != data[i, j])
                        {
                            Console.WriteLine("\ntest_twodim_array: read value differs from input: read {0} - input {1}",
                                outdata[i, j], data[i, j]);
                            nerrors++;
                        }
                    }

                // Close all objects and file.
                H5D.close(dsetId);
                H5T.close(dtypeId);
                H5S.close(dspaceId);
                H5F.close(fileId);

                Console.WriteLine("\tPASSED");
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
        }  // test_twodims_array

        static void test_fivedims_array(H5FileId fileId)
        {
            try
            {
                Console.Write("Testing write/read five-dimensional array");

                const string DSET_NAME = ("Five-dim IntArray");
                const int DIM1 = 1;   // data set dimension
                const int DIM2 = 2;
                const int DIM3 = 2;
                const int DIM4 = 4;
                const int DIM5 = 4;
                const int RANK = 5; // five-dimension

                // Data  and output buffer initialization.
                int i, j, k, m, n;
                int[, , , ,] data = new int[DIM1, DIM2, DIM3, DIM4, DIM5];
                for (i = 0; i < DIM1; i++)
                    for (j = 0; j < DIM2; j++)
                        for (k = 0; k < DIM3; k++)
                            for (m = 0; m < DIM4; m++)
                                for (n = 0; n < DIM5; n++)
                                {
                                    data[i, j, k, m, n] = i + j + k + m + n;
                                }

                // Describe the size of the array and create the data space for fixed
                // size dataset.
                ulong[] dims = { DIM1, DIM2, DIM3, DIM4, DIM5 };
                H5DataSpaceId dspaceId = H5S.create_simple(RANK, dims);

                // Define datatype for the data in the file.
                H5DataTypeId dtypeId = H5T.copy(H5T.H5Type.NATIVE_INT);

                // Create the data set DSET_NAME.
                H5DataSetId dsetId = H5D.create(fileId, DSET_NAME, dtypeId, dspaceId);

                // Write the one-dimensional data set array.
                H5D.write(dsetId, dtypeId, new H5Array<int>(data));

                int[, , , ,] outdata = new int[DIM1, DIM2, DIM3, DIM4, DIM5];
                for (i = 0; i < DIM1; i++)
                    for (j = 0; j < DIM2; j++)
                        for (k = 0; k < DIM3; k++)
                            for (m = 0; m < DIM4; m++)
                                for (n = 0; n < DIM5; n++)
                                {
                                    outdata[i,j,k,m,n] = 0;
                                }

                // Close and re-open the dataset.
                H5D.close(dsetId);
                dsetId = H5D.open(fileId, DSET_NAME);

                // Read back data.
                H5D.read(dsetId, dtypeId, new H5Array<int>(outdata));

                // Compare against input buffer to verify.
                for (i = 0; i < DIM1; i++)
                    for (j = 0; j < DIM2; j++)
                        for (k = 0; k < DIM3; k++)
                            for (m = 0; m < DIM4; m++)
                                for (n = 0; n < DIM5; n++)
                                {
                                    int out_value = outdata[i, j, k, m, n];
                                    int in_value = data[i, j, k, m, n];
                                    if (out_value != in_value)
                                    {
                                        Console.WriteLine("\ntest_fivedim_array: read value differs from input: read {0} - input {1}",
                                            out_value, in_value);
                                        nerrors++;
                                    }
                                }

                // Close all objects and file.
                H5D.close(dsetId);
                H5T.close(dtypeId);
                H5S.close(dspaceId);

                Console.WriteLine("\tPASSED");
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
            try
            {
                const string FILE_NAME = ("Dataset.h5");

                // Suppress error printing from the C library.
                H5E.suppressPrinting();

                // Create a new file using H5F_ACC_TRUNC access,
                // default file creation properties, and default file
                // access properties.
                H5FileId fileId = H5F.create(FILE_NAME, H5F.CreateMode.ACC_TRUNC);

                test_create(fileId);            // test creating dataset
                test_onedim_array(fileId);      // test writing multiple-dimensional arrays
                test_twodims_array();
                test_fivedims_array(fileId);

                // Close the file.
                H5F.close(fileId);
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
            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test(s) failed: ", nerrors, "occurred!");
            else
                Console.WriteLine("All dataset tests passed.");
            Console.WriteLine();
        }
    }
}
