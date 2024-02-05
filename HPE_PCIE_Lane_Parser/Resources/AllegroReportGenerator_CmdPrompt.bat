@ECHO OFF

ECHO -- Script to generate reports from an Allegro layout board file in the current directory --
ECHO -- Written by Vance Trevino --
ECHO:
ECHO -- Please make sure there is only one board file in this directory.
ECHO -- Please make sure that the Allegro application report.exe is in the directory "C:\Cadence\SPB_17.2\tools\bin"
ECHO:

SET CurrentDirectory=%~dp0

for %%f in (*.brd) do (
  if "%%~xf"==".brd" (
    SET BoardFileDirectory=%%~dpnxf
  )
)

ECHO ----------------------------------------
ECHO Current file location: %CurrentDirectory%
ECHO Board file location:   %BoardFileDirectory%
ECHO Running the "report.exe" application from the directory: "C:\Cadence\SPB_17.2\tools\bin"
ECHO  -- If your Cadence library is in a different folder, please change this scripts folder location.
ECHO ----------------------------------------
ECHO:

REM -----------------------------------------------------------------------
REM CHANGE BELOW LINE - If your Cadence directory is not in the C:\ drive. 
REM -----------------------------------------------------------------------
CD /d C:\Cadence\SPB_17.2\tools\bin
REM -----------------------------------------------------------------------

ECHO ----------------------------------------
ECHO Generating Via by Net Report
ECHO Generating Etch Length By Layer Report
ECHO Generating Pin Pair Report --- This may take a while
ECHO ----------------------------------------
ECHO:

ECHO ----------------------------------------
ECHO Now writting reports. Do not exit until three reports have been generated. 
START "" /B %cd%\report.exe -v vialist_net "%BoardFileDirectory%" "%CurrentDirectory%ViaByNetReport.csv"^
          & %cd%\report.exe -v ell "%BoardFileDirectory%" "%CurrentDirectory%EtchLengthByLayerReport.csv"^
          & %cd%\report.exe -v elp "%BoardFileDirectory%" "%CurrentDirectory%PinPairReport.csv"
ECHO ----------------------------------------
ECHO:

PAUSE