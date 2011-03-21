using System;
using System.Collections.Generic;
using System.Text;
using HDF5DotNet;

namespace dtypes
{
    using hsize_t = System.UInt64;
    class Program
    {
        const int DIM0 = 3;
        const int DIM1 = 5;
        static int nerrors = 0;
        struct s1 {
            public char c;
            public uint i;
            public long l;
        };

        static void test_classes()
        {
            try
            {
                Console.Write("Testing datatype classes");

                /*-------------------------------------------------------------
                 *  Check class of some atomic types.
                 *-----------------------------------------------------------*/
                H5T.H5TClass tcls = H5T.getClass(H5T.H5Type.NATIVE_INT);
                if (tcls != H5T.H5TClass.INTEGER)
                {
                    Console.WriteLine("Test class should have been H5T_INTEGER");
                    nerrors++;
                }

                tcls = H5T.getClass(H5T.H5Type.NATIVE_DOUBLE);
                if (tcls != H5T.H5TClass.FLOAT)
                {
                    Console.WriteLine("Test class should have been H5T_FLOAT");
                    nerrors++;
                }

                H5DataTypeId op_type = H5T.create(H5T.CreateClass.OPAQUE, 1);
                tcls = H5T.getClass(op_type);
                if (tcls != H5T.H5TClass.OPAQUE)
                {
                    Console.WriteLine("Test class should have been H5T_OPAQUE");
                    nerrors++;
                }

                // Create a VL datatype of char.  It should be a VL, not a string class.
                H5DataTypeId vlcId = H5T.vlenCreate(H5T.H5Type.NATIVE_UCHAR);

                // Make certain that the correct classes can be detected
                tcls = H5T.getClass(vlcId);
                if (tcls != H5T.H5TClass.VLEN)
                {
                    Console.WriteLine("Test class should have been H5T_VLEN");
                    nerrors++;
                }

                // Make certain that an incorrect class is not detected */
                if (tcls == H5T.H5TClass.STRING)
                {
                    Console.WriteLine("Test class should not be H5T_STRING");
                    nerrors++;
                }

                // Create a VL string.  It should be a string, not a VL class.
                H5DataTypeId vlsId = H5T.copy(H5T.H5Type.C_S1);
                H5T.setSize(vlsId, 0xffffffff); // for H5T_VARIABLE
                tcls = H5T.getClass(vlsId);
                if (tcls != H5T.H5TClass.STRING)
                {
                    Console.WriteLine("Test class should have been H5T_STRING");
                    nerrors++;
                }
                if (tcls == H5T.H5TClass.VLEN)
                {
                    Console.WriteLine("Test class should not be H5T_VLEN");
                    nerrors++;
                }
                
                // Close datatype
                H5T.close(vlcId);

                Console.WriteLine("\t\tPASSED");
            } // end of try block
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
        }  // end of test_classes

        const string DSET_ENUM_NAME = "Enum Dataset";
        static void test_enum(H5FileId fileId)
        {
            //void* tmp;
            short i, j;
            string[] mname = { "RED", "GREEN", "BLUE", "YELLOW", "PINK", "PURPLE", "ORANGE", "WHITE" };
            short[,] spoints2 = new short[DIM0, DIM1];
            short[,] scheck2 = new short[DIM0, DIM1];
            try
            {
                Console.Write("Testing enumeration datatypes");

                // Create the data space */
                hsize_t[] dims = { DIM0, DIM1 };
                H5DataSpaceId dspace = H5S.create_simple(2, dims);

                // Construct enum type based on native type
                H5DataTypeId etype = H5T.enumCreate(H5T.H5Type.NATIVE_INT);

                // Insert members to type.
                for (i = 0; i < 8; i++)
                {
                    H5T.enumInsert(etype, mname[i], ref i);
                }

                // Assign a name to the enum type, close it, and open it by name.
                H5T.commit(fileId, "Color Type", etype);
                H5T.close(etype);
                H5DataTypeId color_type = H5T.open(fileId, "Color Type");

                // Check its class
                H5T.H5TClass tcls = H5T.getClass(color_type);
                if (tcls != H5T.H5TClass.ENUM)
                    Console.WriteLine("test_enum: class of color_type = {0} is incorrect, should be ENUM", tcls);

                // Create the dataset
                H5DataSetId dataset = H5D.create(fileId, DSET_ENUM_NAME, color_type, dspace);

                // Construct enum type based on native type in memory.
                H5DataTypeId etype_m = H5T.enumCreate(H5T.H5Type.NATIVE_SHORT);

                // Insert members to type.
                for (i = 0; i < 8; i++)
                {
                    H5T.enumInsert(etype_m, mname[i], ref i);
                }

                // Initialize the dataset and buffer.
                for (i = 0; i < DIM0; i++)
                {
                    for (j = 0; j < DIM1; j++)
                    {
                        spoints2[i, j] = i;
                        scheck2[i, j] = 0;
                    }
                }
                // Write the data to the dataset.
                H5D.write(dataset, etype_m, new H5Array<short>(spoints2));

                // Close objects.
                H5D.close(dataset);
                H5T.close(color_type);
                H5S.close(dspace);
                H5T.close(etype_m);

                // Open dataset again to check the type.
                dataset = H5D.open(fileId, DSET_ENUM_NAME);

                // Get dataset's datatype.
                H5DataTypeId dstype = H5D.getType(dataset);

                // Get the datatype's class and check that it is of class ENUM.
                H5T.H5TClass tclass = H5T.getClass(dstype);
                if (tclass != H5T.H5TClass.ENUM)
                {
                    Console.WriteLine("Type should be an enum class");
                    nerrors++;
                }

                // Read data back.
                H5D.read(dataset, dstype, new H5Array<short>(scheck2));

                // Close objects.
                H5D.close(dataset);
                H5T.close(dstype);

                Console.WriteLine("\t\tPASSED");
            } // end of try block
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
        }  // end of test_enum

        const string DSET_COMPOUND_NAME = "Compound Dataset";
        static void test_compound_dtype(H5FileId fileId)
        {
            uint			i, j, n;
            try
            {
                Console.Write("Testing compound datatypes");

                // Allocate space for the points & check arrays
                s1[,] points = new s1[DIM0,DIM1];
                s1[,] check = new s1[DIM0,DIM1];

                // Initialize the dataset
                for (i = n = 0; i < DIM0; i++) {
	            for (j = 0; j < DIM1; j++) {
	                points[i,j].c = 't';
	                points[i,j].i = n++;
	                points[i,j].l = (i*10+j*100)*n;
	            }
                }

                // Create the data space
                hsize_t[] dims = {DIM0,DIM1};
                H5DataSpaceId spaceId = H5S.create_simple(2, dims);

                // Create compound datatype for disk storage
                H5DataTypeId typeId = H5T.create(H5T.CreateClass.COMPOUND, 16);

                // Insert members
                H5T.insert(typeId, "c", 0, H5T.H5Type.STD_U8LE);
                H5T.insert(typeId, "i", 1, H5T.H5Type.STD_U32LE);
                H5T.insert(typeId, "l", 5, H5T.H5Type.STD_I64BE);

                // Create the dataset
                H5DataSetId dsetId = H5D.create(fileId, DSET_COMPOUND_NAME, typeId, spaceId);

                // Write the dataset
                H5D.write(dsetId, typeId, new H5Array<s1>(points));

                // Close dataset and dataspace
                H5D.close(dsetId);
                H5S.close(spaceId);
                H5T.close(typeId);

                // Open dataset again to check various functions.
                dsetId = H5D.open(fileId, DSET_COMPOUND_NAME);

                // Get its type and native type.
                H5DataTypeId dset_typeId = H5D.getType(dsetId);
                H5DataTypeId native_type = H5T.getNativeType(dset_typeId, H5T.Direction.DEFAULT);

                // Check name against this list
                string[] memb_names = { "c", "i", "l" };

                H5DataTypeId mtypeId;   // member type
                H5T.H5TClass memb_cls1, memb_cls2;  // member classes retrieved different ways
                string memb_name;       // member name
                int memb_idx;           // member index

                // Get the number of members in the type.
                int nmembers = H5T.getNMembers(native_type);

                // For each member, check its name, class, index, and size.
                for (i = 0; i < nmembers; i++)
                {
                    // Get the type of the ith member.
                    mtypeId = H5T.getMemberType(native_type, i);

                    // Get the name of the ith member.
                    memb_name = H5T.getMemberName(native_type, i);
                    if (memb_name != memb_names[i])
                    {
                        Console.WriteLine("test_compound_dtypes: incorrect member name, {0}, for member no {1}", memb_name, i);
                        nerrors++;
                    }

                    // Get the class of the ith member and then verify the class.
                    memb_cls1 = H5T.getMemberClass(native_type, i);
                    if (memb_cls1 != H5T.H5TClass.INTEGER)
                    {
                        Console.WriteLine("test_compound_dtypes: incorrect class, {0}, for member no {1}", memb_cls1, i);
                        nerrors++;
                    }

                    // Get the class via type id
                    memb_cls2 = H5T.getClass(mtypeId);
                    if (memb_cls1 != memb_cls2)
                    {
                        Console.WriteLine("test_compound_dtypes: H5T.getMemberClass and H5T.getClass return different classes for the same type.");
                        nerrors++;
                    }

                    // Get member's index back from its name and verify it.
                    memb_idx = H5T.getMemberIndex(dset_typeId, memb_name);
                    if (memb_idx != i)
                    {
                        Console.WriteLine("test_compound_dtypes: H5T.getMemberName and/or H5T.getMemberIndex returned false values.");
                        nerrors++;
                    }

                    // Get size of the member's type and verify it.
                    uint tsize = H5T.getSize(mtypeId);
                    switch (i)
                    {
                        case 0:
                            //Console.WriteLine("tsize = {0}, STD_U8LE = {1}", tsize, H5T.getSize(H5T.H5Type.STD_U8LE));
                            if (tsize != H5T.getSize(H5T.H5Type.STD_U8LE))
                            {
                                Console.WriteLine("test_compound_dtypes: First member has incorrect size");
                                nerrors++;
                            }
                            break;
                        case 1:
                            if (tsize != H5T.getSize(H5T.H5Type.STD_U32LE))
                            {
                                Console.WriteLine("test_compound_dtypes: Second member has incorrect size");
                                nerrors++;
                            }
                            break;
                        case 2:
                            if (tsize != H5T.getSize(H5T.H5Type.STD_I64BE))
                            {
                                Console.WriteLine("test_compound_dtypes: Third member has incorrect size");
                                nerrors++;
                            }
                            break;
                        default:
                            Console.WriteLine("test_compound_dtypes: Only 3 members.");
                            break;
                    }  // end switch

                    // Close current member type.
                    H5T.close(mtypeId);
                }  // end for

                // Close objects.
                H5T.close(dset_typeId);
                H5T.close(native_type);
                H5D.close(dsetId);

                Console.WriteLine("\t\tPASSED");
            } // end of try block
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
        } // test_compound_dtype

        static void test_copy(H5FileId fileId)
        {
            try
            {
                Console.Write("Testing copying datatypes");

                // Make a copy of a predefined type.
                H5DataTypeId inttype = H5T.copy(H5T.H5Type.NATIVE_INT);
                uint intsize = H5T.getSize(inttype);

                // Make a copy of that type.
                H5DataTypeId tcopy1 = H5T.copy(inttype);
                uint tcopy1_size = H5T.getSize(tcopy1);

                // The sizes of the copies should be the same.
                if (intsize != tcopy1_size)
                {
                    Console.WriteLine("test_copy: copy types incorrectly");
                    nerrors++;
                }

                // Close second type
                H5T.close(tcopy1);

                /*
                 * Test copy a datatype from a dataset.
                 */
                // Create a dataset with a simple dataspace.
                hsize_t[] dims = { DIM0, DIM1 };
                H5DataSpaceId dspace = H5S.create_simple(2, dims);
                H5DataSetId dset = H5D.create(fileId, "test_types", inttype, dspace);

                // Obtain the datatype from the dataset.
                H5DataTypeId dstype = H5T.copy(dset);

                // Check this datatype's class, size, and sign.
                H5T.H5TClass tclass = H5T.getClass(dstype);
                if (tclass != H5T.H5TClass.INTEGER)
                {
                    Console.WriteLine("test_copy: copy of dataset's datatype has incorrect class");
                    nerrors++;
                }

                uint tsize = H5T.getSize(dstype);
                if (tsize != intsize)
                {
                    Console.WriteLine("test_copy: copy of dataset's datatype has incorrect size");
                    nerrors++;
                }

                H5T.Sign tsign = H5T.getSign(dstype);
                if (tsign != H5T.Sign.TWOS_COMPLEMENT)
                {
                    Console.WriteLine("test_copy: copy of dataset's datatype has incorrect sign, {0}", tsign);
                    nerrors++;
                }

                // Close objects.
                H5T.close(inttype);
                H5S.close(dspace);
                H5T.close(dstype);
                H5D.close(dset);

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
        } // test_copy

        static void Main(string[] args)
        {
            // Create the file
            H5FileId fileId = H5F.create("test_types.h5", H5F.CreateMode.ACC_TRUNC);

            // Invokes individual datatype tests
            test_classes();
            test_enum(fileId);
            test_copy(fileId);
            test_compound_dtype(fileId);

            // Close the file
            H5F.close(fileId);

            // Report results.
            Console.WriteLine();
            if (nerrors > 0)
                Console.WriteLine("Test(s) failed: {0}", nerrors, "occurred!");
            else
                Console.WriteLine("All datatype tests passed.");
            Console.WriteLine();
        }
    }
}
