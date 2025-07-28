#!/bin/bash

echo "🚀 Starting Day 1 API Learning..."
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

# Build and start the API
echo "🔄 Building and starting the API..."
docker-compose up --build

echo
echo "🎉 API is running at http://localhost:8001"
echo "📖 Documentation: http://localhost:8001/docs"
echo 