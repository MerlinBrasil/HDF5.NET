@ECHO OFF
echo ================================
echo Testing C# API tests - limited
echo ================================

echo cd h5files\bin\Debug
cd h5files\bin\Debug
h5files.exe

cd ..\..\..\
echo cd h5groups\bin\Debug
cd h5groups\bin\Debug
h5groups.exe

cd ..\..\..\
echo cd dsets\bin\Debug
cd dsets\bin\Debug
dsets.exe

cd ..\..\..\
echo cd dspace\bin\Debug
cd dspace\bin\Debug
dspace.exe

cd ..\..\..\
echo cd dtypes\bin\Debug
cd dtypes\bin\Debug
dtypes.exe

cd ..\..\..\





