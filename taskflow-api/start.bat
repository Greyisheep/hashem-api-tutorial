@echo off
echo 🚀 Starting Day 1 API Learning...
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not installed or not running
    echo Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)

REM Check if docker-compose is available
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker Compose is not available
    echo Please ensure Docker Desktop is running
    pause
    exit /b 1
)

echo ✅ Docker is available
echo.

REM Build and start the API
echo 🔄 Building and starting the API...
docker-compose up --build

echo.
echo 🎉 API is running at http://localhost:8001
echo 📖 Documentation: http://localhost:8001/docs
echo.
pause 