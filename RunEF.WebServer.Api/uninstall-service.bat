@echo off
echo Uninstalling RunEF WebServer API Windows Service...
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

:: Check if service exists
echo Checking if service exists...
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorLevel% == 0 (
    echo Service found. Proceeding with uninstallation...
) else (
    echo Service "%SERVICE_NAME%" not found.
    echo Nothing to uninstall.
    goto :end
)

:: Stop the service
echo Stopping service...
sc stop "%SERVICE_NAME%"
if %errorLevel% == 0 (
    echo Service stopped successfully.
) else (
    echo Service may already be stopped or failed to stop.
)

:: Wait a moment for service to fully stop
echo Waiting for service to stop completely...
timeout /t 3 /nobreak >nul

:: Delete the service
echo Removing service...
sc delete "%SERVICE_NAME%"
if %errorLevel% == 0 (
    echo.
    echo SUCCESS: RunEF WebServer API service has been uninstalled!
    echo Service "%SERVICE_NAME%" has been removed from the system.
) else (
    echo ERROR: Failed to remove service. The service may still be running.
    echo Try stopping all related processes and run this script again.
)

:end
echo.
echo Press any key to exit...
pause >nul