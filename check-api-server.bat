@echo off
echo ========================================
echo Kiem tra va khac phuc van de API Server
echo ========================================
echo.

echo 1. Kiem tra Windows Desktop Runtime...
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App"
if %errorlevel% neq 0 (
    echo ERROR: Windows Desktop Runtime chua duoc cai dat hoac khong tim thay
    echo Hay cai dat: windowsdesktop-runtime-9.0.8-win-x64.exe
    pause
    exit /b 1
)
echo Windows Desktop Runtime da duoc cai dat
echo.

echo 2. Kiem tra port 5265 co bi chiem khong...
netstat -an | findstr ":5265"
if %errorlevel% equ 0 (
    echo WARNING: Port 5265 da duoc su dung
    echo Hay kiem tra va dung process khac neu can
    echo.
)

echo 3. Kiem tra firewall...
netsh advfirewall firewall show rule name="RunEF API Server" >nul 2>&1
if %errorlevel% neq 0 (
    echo Tao rule firewall cho API Server...
    netsh advfirewall firewall add rule name="RunEF API Server" dir=in action=allow protocol=TCP localport=5265
    netsh advfirewall firewall add rule name="RunEF API Server Out" dir=out action=allow protocol=TCP localport=5265
    echo Da tao rule firewall
) else (
    echo Rule firewall da ton tai
)
echo.

echo 4. Kiem tra SQL Server connection...
echo Test ket noi database...
sqlcmd -S 172.19.111.246 -U sa -P 134679 -Q "SELECT 1" -t 10
if %errorlevel% neq 0 (
    echo ERROR: Khong the ket noi den SQL Server
    echo Hay kiem tra:
    echo - SQL Server co dang chay khong
    echo - Username/Password co dung khong
    echo - Network connection co on dinh khong
    pause
    exit /b 1
)
echo Ket noi database thanh cong
echo.

echo 5. Chay API Server voi logging chi tiet...
echo Dang khoi dong API Server...
echo.
echo Neu server khong chay, hay kiem tra:
echo - Log file trong thu muc logs
echo - Event Viewer > Windows Logs > Application
echo - Chay voi quyen Administrator neu can
echo.

set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://0.0.0.0:5265

echo Chay voi URL: %ASPNETCORE_URLS%
echo Nhan Ctrl+C de dung server
echo.

RunEF.WebServer.Api.exe --urls "http://0.0.0.0:5265" --environment Development

pause
