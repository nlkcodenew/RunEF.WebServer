@echo off
echo ========================================
echo    Testing RunEF API Endpoints
echo ========================================

set API_BASE=http://localhost:5000
set API_URL=%API_BASE%/api

echo.
echo 1. Testing Health Check...
curl -s %API_BASE%/health
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Health check failed!
    echo Make sure API server is running on port 5000
    pause
    exit /b 1
)

echo.
echo 2. Testing Swagger UI...
curl -s -I %API_BASE%/
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Swagger UI not accessible!
)

echo.
echo 3. Testing Activity Log Endpoint (Unauthorized)...
curl -s -X POST %API_URL%/ActivityLog -H "Content-Type: application/json" -d "{\"computerCode\":\"TEST-PC\",\"ipAddress\":\"192.168.1.1\",\"activityType\":\"TEST\",\"description\":\"Test log\",\"additionalData\":\"Test\"}"

echo.
echo ========================================
echo    API Testing Complete
echo ========================================
echo.
echo Open browser: %API_BASE%/ for Swagger UI
echo.
echo Next steps:
echo 1. Start RunEF-Client application
echo 2. Monitor logs for API integration
echo 3. Check database for activity logs
echo.

pause
