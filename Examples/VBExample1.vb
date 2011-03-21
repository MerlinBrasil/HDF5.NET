using namespace System;
using namespace HDF5DotNet;

int main()
{
   try
   {
      // We will write and read an int array of this length.
      const int DATA_ARRAY_LENGTH = 128;

      // Rank is the number of dimensions of the data array.
      const int RANK = 1;

      // Create an HDF5 file.
      // The enumeration type H5F.CreateMode provides only the legal 
      // creation modes.  Missing H5Fcreate parameters are provided
      // with default values.
      H5FileId^ fileId = H5F::create("myCpp.h5", 
				     H5F::CreateMode::ACC_TRUNC);
  
      // Create a HDF5 group.  
      H5GroupId^ groupId = H5G::create(fileId, "/cppGroup", 0);

      // Prepare to create a data space for writing a 1-dimensional 
      // signed integer array.
      array<unsigned long long>^ dims = 
		   gcnew array<unsigned long long>(RANK);
      dims[0] = DATA_ARRAY_LENGTH;

      // Put descending ramp data in an array so that we can
      // write it to the file.
      array<int>^ dset_data = gcnew array<int>(DATA_ARRAY_LENGTH);
      for (int i = 0; i < DATA_ARRAY_LENGTH; i++)
	 dset_data[i] = DATA_ARRAY_LENGTH - i;

      // Create a data space to accommodate our 1-dimensional array.
      // The resulting H5DataSpaceId will be used to create the 
      // data set.
      H5DataSpaceId^ spaceId = H5S::create_simple(RANK, dims, 0);

      // Create a copy of a standard data type.  We will use the 
      // resulting H5DataTypeId to create the data set.  We could
      // have  used the HST.STD data directly in the call to 
      // H5D.create, but this demonstrates the use of H5T.copy 
      // and the use of a H5DataTypeId in H5D.create.
      H5DataTypeId^ typeId = H5T::copy(H5T::STD::I32BE);

      // Create the data set.
      H5DataSetId^ dataSetId = H5D::create(fileId, "/csharpExample", 
					 typeId, spaceId);

      // Write the integer data to the data set.
      H5D::write(dataSetId, H5T::STD::I32BE, dset_data);

      // Create an integer array to receive the read data.
      array<int>^ readDataBack = gcnew array<int>(DATA_ARRAY_LENGTH);

      // Read the integer data back from the data set
      H5D::read(dataSetId, H5T::STD::I32BE, readDataBack);

      // Echo the data
      for(int i=0;i<DATA_ARRAY_LENGTH;i++)
      {
	 Console::WriteLine(readDataBack[i]);
      }

      // Close all the open resources.
      H5D::close(dataSetId);
      H5S::close(spaceId);
      H5T::close(typeId);
      H5G::close(groupId);
      H5F::close(fileId);

      // Reopen and reclose the file.
      H5FileId^ openId = H5F::open("myCpp.h5", 
				 H5F::OpenMode::ACC_RDONLY);
      H5F::close(openId);
   }
   // This catches all the HDF exception classes.  Because each call
   // generates unique exception, different exception can be handled
   // separately.  For example, to catch open errors we could have used
   // catch (H5FopenException^ openException).
   catch (HDFException^ e)
   {
      Console::WriteLine(e->Message);
   }
   Console::WriteLine("Processing complete!");
   Console::ReadLine();
}
