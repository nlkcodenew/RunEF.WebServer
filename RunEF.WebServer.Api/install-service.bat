@echo off
echo Installing RunEF WebServer API as Windows Service...
echo.

:: Check if running as administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with administrator privileges.
) else (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

:: Set service details
set SERVICE_NAME=RunEFWebServerAPI
set SERVICE_DISPLAY_NAME=RunEF WebServer API
set SERVICE_DESCRIPTION=RunEF WebServer API Service for client management
set EXECUTABLE_PATH=%~dp0RunEF.WebServer.Api.exe
set WORKING_DIRECTORY=%~dp0

:: Check if executable exists
if not exist "%EXECUTABLE_PATH%" (
    echo ERROR: Executable not found at %EXECUTABLE_PATH%
    echo Please make sure the application is published to this directory.
    pause
    exit /b 1
)

:: Stop service if it exists
echo Stopping existing service if running...
sc stop "%SERVICE_NAME%" >nul 2>&1

:: Delete existing service
echo Removing existing service if exists...
sc delete "%SERVICE_NAME%" >nul 2>&1

:: Create the service
echo Creating Windows Service...
sc create "%SERVICE_NAME%" ^
    binPath= "\"%EXECUTABLE_PATH%\" --environment=Production" ^
    DisplayName= "%SERVICE_DISPLAY_NAME%" ^
    start= auto ^
    obj= "LocalSystem"

if %errorLevel% == 0 (
    echo Service created successfully.
) else (
    echo ERROR: Failed to create service.
    pause
    exit /b 1
)

:: Set service description
echo Setting service description...
sc description "%SERVICE_NAME%" "%SERVICE_DESCRIPTION%"

:: Configure service recovery options
echo Configuring service recovery options...
sc failure "%SERVICE_NAME%" reset= 86400 actions= restart/5000/restart/5000/restart/5000

:: Start the service
echo Starting service...
sc start "%SERVICE_NAME%"

if %errorLevel% == 0 (
    echo.
    echo SUCCESS: RunEF WebServer API service has been installed and started!
    echo Service Name: %SERVICE_NAME%
    echo Display Name: %SERVICE_DISPLAY_NAME%
    echo API URL: http://localhost:5265
    echo.
    echo You can manage this service using:
    echo - Services.msc (Windows Services Manager)
    echo - sc start "%SERVICE_NAME%" (to start)
    echo - sc stop "%SERVICE_NAME%" (to stop)
    echo - sc query "%SERVICE_NAME%" (to check status)
) else (
    echo ERROR: Failed to start service. Check Windows Event Log for details.
)

echo.
echo Press any key to exit...
pause >nul