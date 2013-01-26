@ECHO OFF
::: -- Prepare the processor --
@SETLOCAL ENABLEEXTENSIONS 
@SETLOCAL ENABLEDELAYEDEXPANSION 

:: -- Version History --
::           Version       YYYYMMDD Author         Description
SET "version=0.0.1"      &:20120729 Phillip Clark  initial version 
SET "title=Build (%~nx0) - %version%"
TITLE %title%

CALL "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"

SET "DISPOSITION=DISPOSITION UNKNOWN"
SET "UNIQUE=unknown"
CALL:make_timestamp UNIQUE

IF "%~1" NEQ "" (
	CALL :ParseCommandLineArg "%~1"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~1
	CALL:PrintUsage
	GOTO :FAIL
)
IF "%~2" NEQ "" (
	CALL :ParseCommandLineArg "%~2"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~2
	CALL:PrintUsage
	GOTO :FAIL
)
IF "%~3" NEQ "" (
	CALL :ParseCommandLineArg "%~3"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~3
	CALL:PrintUsage
	GOTO :FAIL
)

IF "%CFG%" == "" (
	SET "CFG=Debug"
)
IF "%PLT%" == "" (
	SET PLT="AnyCPU"
)
IF "%VRB%" == "" (
	SET VRB="detailed"
)

FOR %%I IN (*.csproj) DO CALL :build_csproj "%%~nxI"	
SET "DISPOSITION=Success!"
GOTO:EXIT

:build_csproj
SET F="%~1"
ECHO.%~p1
ECHO. %~1
SET "CL=msbuild %F% /p:Configuration=%CFG%;Platform=%PLT%;BuildPackage=true /t:Build /v:%VRB% > build_%UNIQUE%.log"
%CL%
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Build failed...
	GOTO:EXIT
)

GOTO:EOF

:ParseCommandLineArg -- Parses a command line argument and sets the corresponding variable
::                   -- %~1: the argument
SETLOCAL
SET C=%~1
SET A=%C:~0,3%
SET V=%C:~3%
SET "G="
IF "%A%" == "/p:" (
	SET "G=PLT"
) ELSE IF "%A%"	== "/P:" (
	SET "G=PLT"
) ELSE IF "%A%"	== "/c:" (
	SET "G=CFG"
) ELSE IF "%A%"	== "/C:" (
	SET "G=CFG"
) ELSE IF "%A%"	== "/v:" (
	SET "G=VRB"
) ELSE IF "%A%"	== "/V:" (
	SET "G=VRB"
) ELSE (
	VERIFY OTHER 2> NUL
)
ENDLOCAL&SET "%G%=%V%"
GOTO:EOF

:make_timestamp -- creates a timestamp and returns it's value in the variable given
::              -- %~1: reference to a variable to hold the timestamp
FOR /f "tokens=2-8 delims=/:. " %%A IN ("%date%:%time: =0%") DO SET "%~1=%%C%%A%%B_%%D%%E%%F%%G"
GOTO:EOF


:FAIL
::    -- %1: the current log file
::    -- %2: a failure message
:: CALL:log %~1 "%~2"
SET "DISPOSITION=FAILED"
COLOR 47
GOTO :EXIT

:EXIT -- Displays the disposition and exits.
ECHO.
SET /P WAIT_RESULT=Script complete: %DISPOSITION% (enter to continue)
GOTO:EOF
