@echo off
echo 🚀 Starting REAL gRPC Demo - TaskFlow Analytics Service
echo =======================================================
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

echo ✅ Docker is available and running
echo.

REM Stop any existing containers
echo 🧹 Cleaning up any existing containers...
docker-compose -f docker-compose-simple.yml down 2>nul || echo No containers to clean up

echo.
echo 🚀 Starting REAL gRPC server and client...
echo 📊 Server will be available on localhost:50051
echo 🧪 Client will run REAL gRPC tests automatically
echo ⏱️  This will take a moment to install dependencies...
echo.

REM Start the services
docker-compose -f docker-compose-simple.yml up --build

echo.
echo 🎉 REAL gRPC Demo completed!
echo 💡 You just saw:
echo   - REAL gRPC server-client communication
echo   - ACTUAL protocol buffer code generation
echo   - REAL streaming data flow
echo   - ACTUAL performance differences
pause 