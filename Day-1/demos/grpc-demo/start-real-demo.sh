#!/bin/bash

echo "🚀 Starting REAL gRPC Demo - TaskFlow Analytics Service"
echo "======================================================="
echo

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed"
    echo "Please install Docker from https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running"
    echo "Please start Docker Desktop or Docker daemon"
    exit 1
fi

# Check if docker-compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not available"
    echo "Please install Docker Compose"
    exit 1
fi

echo "✅ Docker is available and running"
echo

# Stop any existing containers
echo "🧹 Cleaning up any existing containers..."
docker-compose -f docker-compose-simple.yml down 2>/dev/null || true

echo
echo "🚀 Starting REAL gRPC server and client..."
echo "📊 Server will be available on localhost:50051"
echo "🧪 Client will run REAL gRPC tests automatically"
echo "⏱️  This will take a moment to install dependencies..."
echo

# Start the services
docker-compose -f docker-compose-simple.yml up --build

echo
echo "🎉 REAL gRPC Demo completed!"
echo "💡 You just saw:"
echo "  - REAL gRPC server-client communication"
echo "  - ACTUAL protocol buffer code generation"
echo "  - REAL streaming data flow"
echo "  - ACTUAL performance differences" 