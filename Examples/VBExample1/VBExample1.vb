Imports HDF5DotNet

Module Module1

    Sub Main()

        Try
            ' We will write and read an int array of this length.
            Dim DATA_ARRAY_LENGTH As Integer = 128

            ' Rank is the number of dimensions of the data array.
            Dim RANK As Integer = 1

            ' Create an HDF5 file.
            ' The enumeration type H5F.CreateMode provides only the legal 
            ' creation modes.  Missing H5Fcreate parameters are provided
            ' with default values.
            Dim fileId As H5FileId
            fileId = H5F.create("myVBFile.h5", H5F.CreateMode.ACC_TRUNC)

            ' Create a HDF5 group.  
            Dim groupId As H5GroupId
            groupId = H5G.create(fileId, "/vbGroup", 0)

            ' Prepare to create a data space for writing a 1-dimensional 
            ' signed integer array.
            Dim dims(RANK) As ULong
            dims(0) = DATA_ARRAY_LENGTH

            ' Put descending ramp data in an array so that we can
            ' write it to the file.
            Dim dset_data(DATA_ARRAY_LENGTH) As Integer

            Dim i
            For i = 0 To DATA_ARRAY_LENGTH - 1
                dset_data(i) = (DATA_ARRAY_LENGTH - i)
            Next i

            ' Create a data space to accommodate our 1-dimensional array.
            ' The resulting H5DataSpaceId will be used to create the 
            ' data set.
            Dim spaceId As H5DataSpaceId
            spaceId = H5S.create_simple(RANK, dims)

            ' Create a copy of a standard data type.  We will use the 
            ' resulting H5DataTypeId to create the data set.  We could
            ' have  used the HST.H5Type data directly in the call to 
            ' H5D.create, but this demonstrates the use of H5T.copy 
            ' and the use of a H5DataTypeId in H5D.create.
            Dim typeId As H5DataTypeId
            typeId = H5T.copy(H5T.H5Type.NATIVE_INT)

            ' Create the data set.
            Dim dataSetId As H5DataSetId
            dataSetId = H5D.create(fileId, "/vbExample", typeId, spaceId)

            ' Write the integer data to the data set.
            Dim h5Data As New H5Array(Of Integer)(dset_data)

            H5D.write(dataSetId, typeId, h5Data)

            ' Create an integer array to receive the read data.
            Dim readDataBack(DATA_ARRAY_LENGTH) As Integer

            Dim h5DataBack As New H5Array(Of Integer)(readDataBack)

            ' Read the integer data back from the data set
            H5D.read(dataSetId, typeId, h5DataBack)

            ' Echo the data
            For i = 0 To DATA_ARRAY_LENGTH - 1
                Console.WriteLine(readDataBack(i))
            Next

            ' Close all the open resources.
            H5D.close(dataSetId)
            H5S.close(spaceId)
            H5T.close(typeId)
            H5G.close(groupId)
            H5F.close(fileId)

            ' Reopen and reclose the file.
            Dim openId As H5FileId
            openId = H5F.open("myVBFile.h5", H5F.OpenMode.ACC_RDONLY)
            H5F.close(openId)

            ' This catches all the HDF exception classes.  Because each call
            ' generates unique exception, different exception can be handled
            ' separately.  For example, to catch open errors we could have used
            ' catch (H5FopenException^ openException).

        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

        Console.WriteLine("Processing complete!")
        Console.ReadLine()

    End Sub

End Module
