@echo off

echo 🚀 Starting GraphQL Demo for TaskFlow API
echo ==========================================

:: Check if .NET 8 SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET 8 SDK is required but not installed.
    echo Please install from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

:: Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is required but not running.
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo ✅ Prerequisites checked

:: Create GraphQL project if it doesn't exist
if not exist "TaskFlow.GraphQL" (
    echo 📦 Creating new GraphQL project...
    dotnet new webapi -n TaskFlow.GraphQL
    cd TaskFlow.GraphQL
    
    echo 📦 Adding GraphQL packages...
    dotnet add package HotChocolate.AspNetCore --version 13.9.0
    dotnet add package HotChocolate.Data.EntityFramework --version 13.9.0
    dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.0.0
    
    cd ..
) else (
    echo ✅ GraphQL project already exists
)

:: Start the main TaskFlow API in the background
echo 🔧 Starting TaskFlow API...
cd ..\..\taskflow-api-dotnet
docker-compose up -d >nul 2>&1

:: Wait for API to be ready
echo ⏳ Waiting for TaskFlow API to be ready...
:wait_loop
curl -s http://localhost:5000/health >nul 2>&1
if %errorlevel% neq 0 (
    timeout /t 2 /nobreak >nul
    goto wait_loop
)
echo ✅ TaskFlow API is ready

:: Start GraphQL server
cd ..\Day-2\demos\graphql-demo\TaskFlow.GraphQL
echo 🚀 Starting GraphQL server...
echo.
echo GraphQL Playground will be available at: http://localhost:5001/graphql
echo REST API for comparison at: http://localhost:5000/swagger
echo.
echo Press Ctrl+C to stop the demo
echo.

dotnet run 