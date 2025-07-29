@echo off
echo ========================================
echo TaskFlow API - .NET Implementation
echo ========================================
echo.
echo Starting TaskFlow API with Docker...
echo.

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo Building and starting services...
docker-compose up --build

echo.
echo ========================================
echo TaskFlow API is now running!
echo ========================================
echo.
echo API Endpoints:
echo - API: http://localhost:5000
echo - Swagger UI: http://localhost:5000/swagger
echo - Health Check: http://localhost:5000/health
echo.
echo Database:
echo - PostgreSQL: localhost:5432
echo - pgAdmin: http://localhost:5050 (admin@taskflow.com / admin123)
echo.
echo Logging:
echo - Seq: http://localhost:5341
echo.
echo Press Ctrl+C to stop the services...
pause 