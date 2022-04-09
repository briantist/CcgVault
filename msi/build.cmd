@echo off

If [%~1] == [] (
    echo "No version specified. Provide a version argument like 1.10.3"
    Goto :EOF
)

pushd "%~dp0\.."
setlocal

set PATH=%WIX%bin;%PATH%
set VER=%~1
set OUT=msi\out
set FRAG=msi\src\Release.wxs

del /F /Q "%FRAG%"
del /F /Q "%OUT%\*.*"

heat.exe dir src\CcgVault\bin\Release -out %FRAG% -suid -ag -sreg -srd -cg CoGr -dr APPLICATIONROOTDIRECTORY -var var.SourceDir
candle.exe msi\src\*.wxs -dCcgVaultVer=%VER% -dSourceDir=src\CcgVault\bin\Release -o msi\out\
light.exe -sval msi\out\*.wixobj -o msi\out\ccgvault_v%VER%_x64.msi

endlocal
popd
