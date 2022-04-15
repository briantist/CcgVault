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
set SRC=msi\src
set GEN=%SRC%\gen
set FRAG=%GEN%\Release.wxs

FOR /F "tokens=* USEBACKQ" %%C IN (`powershell.exe -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -File msi\PSEncode.ps1 -Path "%SRC%\TerminateCcgVault.ps1"`) DO (
    set KillCmd=%%C
)

echo KillCmd=%KillCmd%

del /F /Q "%GEN%\*.wxs"
del /F /Q "%OUT%\*.*"

heat.exe dir src\CcgVault\bin\Release -out %FRAG% -suid -ag -sreg -srd -cg CoGr -dr APPLICATIONROOTDIRECTORY -var var.SourceDir -t %SRC%\HeatFilter.xsl
candle.exe -ext WixComPlusExtension %SRC%\*.wxs %GEN%\*.wxs -dKillCmd=%KillCmd% -dCcgVaultVer=%VER% -dSourceDir=src\CcgVault\bin\Release -o msi\out\
light.exe -ext WixUtilExtension.dll -ext WixComPlusExtension -sval %OUT%\*.wixobj -o %OUT%\ccgvault_v%VER%_x64.msi

endlocal
popd
