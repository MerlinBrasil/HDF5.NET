using System;
using System.Collections.Generic;
using System.Text;
using HDF5DotNet;

namespace dspace
{
    using hsize_t = System.UInt64;
    using hssize_t = System.Int64;
    class Program
    {
        static int nerrors = 0;

        // Various datasets and files
        const string DATAFILE = "th5s1.h5";
        const string BASICFILE = "th5s3.h5";
        const string BASICDATASET = "basic_dataset";
        const string BASICDATASET2 = "basic_dataset2";

        // 3-D dataset
        const int SPACE1_RANK = 3;
        const int SPACE1_DIM1 = 3;
        const int SPACE1_DIM2 = 15;
        const int SPACE1_DIM3 = 13;

        // 4-D dataset
        const int SPACE2_RANK = 4;
        const int SPACE2_DIM1 = 1;
        const int SPACE2_DIM2 = 15;
        const int SPACE2_DIM3 = 13;
        const int SPACE2_DIM4 = 23;
        const int SPACE2_MAX1 = 1;
        const int SPACE2_MAX2 = 15;
        const int SPACE2_MAX3 = 13;
        const int SPACE2_MAX4 = 23;

        static void test_h5s_basic()
        {
            try
            {
                int rank;		// Logical rank of dataspace
                hsize_t[] dims1 = {SPACE1_DIM1, SPACE1_DIM2, SPACE1_DIM3};
                hsize_t[] dims2 = {SPACE2_DIM1, SPACE2_DIM2, SPACE2_DIM3, SPACE2_DIM4};
                hsize_t[] max2 = {SPACE2_MAX1, SPACE2_MAX2, SPACE2_MAX3, SPACE2_MAX4};
                hsize_t[] tmax = new hsize_t[4];

                // Output message about test being performed.
                Console.Write("Testing Dataspace Manipulation");

                // Create a simple dataspace and check its rank.
                H5DataSpaceId sid1 = H5S.create_simple(SPACE1_RANK, dims1);
                rank = H5S.getSimpleExtentNDims(sid1);
                if (rank != SPACE1_RANK)
                {
                    Console.WriteLine("\ntest_h5s_basic: Incorrect rank {0}, should be SPACE1_RANK({1})",
                                        rank, SPACE1_RANK);
                    nerrors++;
                }

                // Check its dims.
                hsize_t[] tdims1 = new hsize_t[3];
                tdims1 = H5S.getSimpleExtentDims(sid1);
                int i;
                for (i = 0; i < rank; i++)
                {
                    if (tdims1[i] != dims1[i])
                    {
                        Console.WriteLine("\ntest_h5s_basic: read tdims1[{0}] = {1} differs from dims1[{0}] = {2}", i, tdims1[i], dims1[i]);
                        nerrors++;
                    }
                }

                // Create another simple dataspace and check its rank, dims, and maxdims.
                H5DataSpaceId sid2 = H5S.create_simple(SPACE2_RANK, dims2, max2);

                rank = H5S.getSimpleExtentNDims(sid2);
                if (rank != SPACE2_RANK)
                {
                    Console.WriteLine("\ntest_h5s_basic: Incorrect rank {0}, should be SPACE1_RANK({1})",
                                        rank, SPACE1_RANK);
                    nerrors++;
                }

                hsize_t[] tdims2 = new hsize_t[3];
                tdims2 = H5S.getSimpleExtentDims(sid2);
                tmax = H5S.getSimpleExtentMaxDims(sid2);

                for (i = 0; i < rank; i++)
                {
                    if (tdims2[i] != dims2[i])
                    {
                        Console.WriteLine("\ntest_h5s_basic: read tdims2[{0}] = {1} differs from dims2[{0}] = {2}", i, tdims2[i], dims2[i]);
                        nerrors++;
                    }
                }

                for (i = 0; i < rank; i++)
                {
                    if (tmax[i] != max2[i])
                    {
                        Console.WriteLine("\ntest_h5s_basic: read tmax[{0}] = {1} differs from max2[{0}] = {2}", i, tmax[i], max2[i]);
                        nerrors++;
                    }
                }

                // Close all dataspaces.
                H5S.close(sid1);
                H5S.close(sid2);

                /*
                 * Try writing simple dataspaces without setting their extents.
                 */
                // Create the file
                H5FileId fid1 = H5F.create(BASICFILE, H5F.CreateMode.ACC_TRUNC);

                // Create dataspaces for testing.
                dims1[0]=SPACE1_DIM1;
                sid1 = H5S.create(H5S.H5SClass.SIMPLE);
                sid2 = H5S.create_simple(1, dims1, dims1);

                // This dataset's space has no extent; it should not be created
                try
                {
                    H5DataSetId dset1 = H5D.create(fid1, BASICDATASET, H5T.H5Type.NATIVE_INT, sid1);

                    // should fail, but didn't, print an error message.
                    Console.WriteLine("\ntest_h5s_basic: Attempting to create a dataset whose space has no extent.");
                    nerrors++;
                }
                catch (H5DcreateException) {} // does nothing, it should fail

                // Create dataset with good dataspace.
                H5DataSetId dataset = H5D.create(fid1, BASICDATASET2, H5T.H5Type.NATIVE_INT, sid2);

                // Try some writes with the bad dataspace (sid1)
                try
                {
                    hssize_t nelems=10; // Number of dataspace elements
                    H5D.writeScalar(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_INT), sid1, sid2, new H5PropertyListId(H5P.Template.DEFAULT), ref nelems);

                    // should fail, but didn't, print an error message.
                    Console.WriteLine("\ntest_h5s_basic: Attempting to write to a dataset with space that has no extent.");
                    nerrors++;
                }
                catch (H5DwriteException) {} // does nothing, it should fail

                // Make sure that dataspace reads using the bad dataspace fail
                try
                {
                    hssize_t n = 10; // Number of dataspace elements
                    H5D.readScalar(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_INT), sid1, sid2, 
                        new H5PropertyListId(H5P.Template.DEFAULT), ref n);

                    // should fail, but didn't, print an error message.
                    Console.WriteLine("\ntest_h5s_basic: Attempting to read a dataset with space that has no extent.");
                    nerrors++;
                }
                catch (H5DreadException) {} // does nothing, it should fail

                // Close objects and file.
                H5D.close(dataset);
                H5S.close(sid1);
                H5S.close(sid2);
                H5F.close(fid1);

                Console.WriteLine("\t\tPASSED");
            } // end of try
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
        } // test_h5s_basic

        // Scalar dataset with simple datatype */
        const int SPACE3_RANK = 0;
        static void test_h5s_scalar()
        {
            try
            {
                hsize_t[] tdims = new hsize_t[3];
                
                // Output message about test being performed.
                Console.Write("Testing Dataspace during Writing");

                // Create the file.
                H5FileId fid1 = H5F.create(DATAFILE, H5F.CreateMode.ACC_TRUNC); 

                // Create scalar dataspace.
                H5DataSpaceId sid1 = H5S.create_simple(SPACE3_RANK, null);

                // Get the logical rank of dataspace and verify it.
                int rank = H5S.getSimpleExtentNDims(sid1);
                if (rank != SPACE3_RANK)
                {
                    Console.WriteLine("\ntest_h5s_scalar: incorrect rank {0}, should be SPACE3_RANK({1})", rank, SPACE3_RANK);
                    nerrors++;
                }

                // Create and write the dataset.
                uint space3_data = 65;
                H5DataSetId dataset = H5D.create(fid1, "Dataset1", H5T.H5Type.NATIVE_UINT, sid1);
                H5D.writeScalar(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_UINT), ref space3_data);

                // Close objects and file.
                H5D.close(dataset);
                H5S.close(sid1);
                H5F.close(fid1);

                /* Open the file and verify the dataspace. */

                // Open the file.
                fid1 = H5F.open(DATAFILE, H5F.OpenMode.ACC_RDWR);

                // Create a dataset.
                dataset = H5D.open(fid1, "Dataset1");

                // Get dataset's dataspace.
                sid1 = H5D.getSpace(dataset);

                rank = H5S.getSimpleExtentNDims(sid1);
                if (rank != SPACE3_RANK)
                    Console.WriteLine("\ntest_h5s_scalar: incorrect rank {0}", rank);

                tdims = H5S.getSimpleExtentDims(sid1);
                //Console.WriteLine("tdims[0] = {0}, tdims[1] = {1}", tdims[0], tdims[1]);
                if (rank != 0)
                    Console.WriteLine("\ntest_h5s_scalar: incorrect rank {0}", rank);

                // Read the dataset.
                uint rdata=0;
                H5D.readScalar(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_UINT), ref rdata);
                if (rdata != space3_data)
                    Console.WriteLine("\ntest_h5s_scalar: incorrect data {0}, should be {1}", rdata, space3_data);

                // Close objects.
                H5D.close(dataset);
                H5S.close(sid1);
                H5F.close(fid1);

                Console.WriteLine("\tPASSED");
            } // end of try
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
        }	// test_h5s_scalar_write

        static void test_select()
        {
            try
            {
                const string FILE_NAME = ("Select.h5");

                const int MSPACE1_RANK = 1;         // Rank of the first dataset in memory
                const int MSPACE1_DIM = 50;         // Dataset size in memory
                const int FSPACE_RANK = 2;          // Dataset rank as it is stored in the file
                const int FSPACE_DIM1 = 8;          // Dimension sizes of the dataset as it is
                const int FSPACE_DIM2 = 12;

                // We will read dataset back from the file to the dataset in memory with these
                // dataspace parameters.
                const int MSPACE_RANK = 2;
                const int MSPACE_DIM1 = 8;
                const int MSPACE_DIM2 = 9;
                int[] vector = new int[MSPACE1_DIM];
                int[] values = new int[4];

                // New values to be written
                values[0] = 53;
                values[1] = 59;
                values[2] = 61;
                values[3] = 67;

                Console.Write("Testing basic selection");

                // Buffers' initialization.
                vector[0] = vector[MSPACE1_DIM - 1] = -1;
                int i, j;
                for (i = 1; i < MSPACE1_DIM - 1; i++) vector[i] = i;

                // Create the file.
                H5FileId fileId = H5F.create(FILE_NAME, H5F.CreateMode.ACC_TRUNC);

                // Create dataspace for the dataset in the file.
                hsize_t[] fdim = {FSPACE_DIM1, FSPACE_DIM2};
                H5DataSpaceId fspace = H5S.create_simple(FSPACE_RANK, fdim);

                // Create dataset in the file.
                H5DataSetId dataset = H5D.create(fileId, "Matrix in file", H5T.H5Type.NATIVE_INT, fspace);

                // Select hyperslab for the dataset in the file, using 3x2 blocks,
                // (4,3) stride and (2,4) count starting at the position (0,1).
                hsize_t[] start = {0, 1};  /* Start of hyperslab */
                hsize_t[] stride = {4, 3};
                hsize_t[] count = {2, 4};
                hsize_t[] block = {3, 2};
                H5S.selectStridedHyperslab(fspace, H5S.SelectOperator.SET, start, stride, count, block);

                // Create dataspace for the dataset.
                hsize_t[] dim1 = {MSPACE1_DIM};  // Dimension size of the first dataset(in memory)
                H5DataSpaceId mspace1 = H5S.create_simple(MSPACE1_RANK, dim1);

                // Select hyperslab.
                // We will use 48 elements of the vector buffer starting at the second element.
                // Selected elements are 1 2 3 ... 48
                start[0] = 1;
                stride[0] = 1;
                count[0] = 48;
                block[0] = 1;
                H5S.selectStridedHyperslab(mspace1, H5S.SelectOperator.SET, start, stride, count, block);

                // Write selection from the vector buffer to the dataset in the file.
                H5D.write(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_INT), mspace1, fspace, 
                    new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<int>(vector));

                // Reset the selection for the file dataspace.
                H5S.selectNone(fspace);

                // Close objects and file.
                H5S.close(mspace1);
                H5S.close(fspace);
                H5D.close(dataset);
                H5F.close(fileId);

                // Reopen the file.
                fileId = H5F.open(FILE_NAME, H5F.OpenMode.ACC_RDONLY);

                // Open the dataset.
                dataset = H5D.open(fileId, "Matrix in file");

                // Get dataspace of the dataset.
                fspace = H5D.getSpace(dataset);

                // Select first hyperslab for the dataset in the file.
                start[0] = 1; start[1] = 2;
                block[0] = 1; block[1] = 1;
                stride[0] = 1; stride[1] = 1;
                count[0]  = 3; count[1]  = 4;
                H5S.selectStridedHyperslab(fspace, H5S.SelectOperator.SET, start, stride, count, block);

                // Add second selected hyperslab to the selection.
                start[0] = 2; start[1] = 4;
                block[0] = 1; block[1] = 1;
                stride[0] = 1; stride[1] = 1;
                count[0]  = 6; count[1]  = 5;
                H5S.selectStridedHyperslab(fspace, H5S.SelectOperator.OR, start, stride, count, block);

                // Create memory dataspace.
                hsize_t[] mdim = { MSPACE_DIM1, MSPACE_DIM2 }; // Dimension sizes of the dataset in memory when we
                // read selection from the dataset on the disk */
                H5DataSpaceId mspace2 = H5S.create_simple(MSPACE_RANK, mdim);

                // Select two hyperslabs in memory. Hyperslabs has the same
                // size and shape as the selected hyperslabs for the file dataspace.
                start[0] = 0; start[1] = 0;
                block[0] = 1; block[1] = 1;
                stride[0] = 1; stride[1] = 1;
                count[0]  = 3; count[1]  = 4;
                H5S.selectStridedHyperslab(mspace2, H5S.SelectOperator.SET, start, stride, count, block);

                start[0] = 1; start[1] = 2;
                block[0] = 1; block[1] = 1;
                stride[0] = 1; stride[1] = 1;
                count[0]  = 6; count[1]  = 5;
                H5S.selectStridedHyperslab(mspace2, H5S.SelectOperator.OR, start, stride, count, block);

                // Initialize data buffer.
                int[,] matrix_out = new int [MSPACE_DIM1, MSPACE_DIM2]; // Buffer to read from the dataset
                for (i = 0; i < MSPACE_DIM1; i++) {
                   for (j = 0; j < MSPACE_DIM2; j++)
                        matrix_out[i,j] = 0;
                }

                int[,] results = {{10,0,11,12,0,0,0,0,0},
                                   {18,0,19,20,0,21,22,0,0},
                                   {0,0,0,0,0,0,0,0,0},
                                   {0,0,27,28,0,29,30,0,0},
                                   {0,0,35,36,0,37,38,0,0},
                                   {0,0,43,44,0,45,46,0,0},
                                   {0,0,0,0,0,0,0,0,0},
                                   {0,0,0,0,0,0,0,0,0}};

                // Read data back to the buffer matrix_out.
                H5D.read(dataset, new H5DataTypeId(H5T.H5Type.NATIVE_INT), mspace2, fspace, 
                    new H5PropertyListId(H5P.Template.DEFAULT), new H5Array<int>(matrix_out));

                // Check read data.
                for (i=0; i < MSPACE_DIM1; i++) {
                    for (j = 0; j < MSPACE_DIM2; j++)
                    {
                        if (matrix_out[i, j] != results[i, j])
                        {
                            Console.WriteLine("test_select: Incorrect read data: {0}, should be {1}", matrix_out[i, j], results[i, j]);
                            nerrors++;
                        }
                    }
                }
                // Close objects and file.
                H5S.close(mspace2);
                H5S.close(fspace);
                H5D.close(dataset);
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
        } // test_select

        static void Main(string[] args)
        {
            // Suppress error printing from the C library.
            H5E.suppressPrinting();

            test_h5s_basic();       // test basic dataspace functionalities
            test_h5s_scalar();      // test scalar dataspace
            test_select();          // test dataspace select

            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test failed: ", nerrors, "occurred!");
            else
                Console.WriteLine("All dataspace tests passed.");
            Console.WriteLine();
        }
    }
}
