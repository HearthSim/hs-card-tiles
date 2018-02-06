@echo off

call bootstrap.bat
msbuild Downloader /t:Downloader /p:Configuration=Debug
.\Downloader\Downloader\bin\Debug\Downloader.exe .