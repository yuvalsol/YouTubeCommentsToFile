@echo off

set project="YouTubeCommentsToFile"
set publishDir="Output"
set win64="win-x64"
set linux64="linux-x64"
set arguments="--configuration Release --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=None -p:DebugSymbols=false --verbosity normal"

:YouTubeCommentsToFile
call :Publish %project%, %publishDir%, "net8.0", %win64%, %arguments%
call :Publish %project%, %publishDir%, "net8.0", %linux64%, %arguments%
:: call :Publish %project%, %publishDir%, "net9.0", %win64%, %arguments%
:: call :Publish %project%, %publishDir%, "net9.0", %linux64%, %arguments%
:: call :Publish %project%, %publishDir%, "net10.0", %win64%, %arguments%
:: call :Publish %project%, %publishDir%, "net10.0", %linux64%, %arguments%

:exit
:: pause
exit /b 0

::----------------------------------------------------------------------

:Publish
:: Publish(project, publishDir, framework, runtime, arguments)
setlocal
set "project=%~1"
set "publishDir=%~2"
set "framework=%~3"
set "runtime=%~4"
set "arguments=%~5"

echo %project% %framework% %runtime%
echo.
:: echo dotnet publish %project% --output "%publishDir%\%framework%\%runtime%" --framework %framework% --runtime %runtime% %arguments%
:: echo.
call dotnet publish %project% --output "%publishDir%\%framework%\%runtime%" --framework %framework% --runtime %runtime% %arguments%
echo.

endlocal
goto :eof
