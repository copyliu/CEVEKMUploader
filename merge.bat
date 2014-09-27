@echo off
del release.exe /f /s /q
@echo mergeing...
ilmerge /out:release.exe /t:winexe /v4 /ndebug %~dp0CEVEKMUploader\bin\Release\CEVEKMUploader.exe %~dp0\CEVEKMUploader\bin\Release\EVECacheParser.dll Newtonsoft.Json.dll
@ehco merge complete.
pause
