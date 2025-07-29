#!/bin/bash

# gRPC Performance Demo Script
# This script compares REST API vs gRPC performance

set -e

echo "🚀 Starting gRPC Performance Demo..."
echo "===================================="

# Check prerequisites
echo "📋 Checking prerequisites..."

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is not installed. Please install Python 3 first."
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

echo "✅ Prerequisites check passed"

# Start the main TaskFlow API (REST)
echo "🌐 Starting TaskFlow REST API..."
cd ../../taskflow-api-dotnet
docker-compose up -d

# Wait for REST API to be healthy
echo "⏳ Waiting for REST API to be ready..."
for i in {1..30}; do
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo "✅ REST API is ready!"
        break
    fi
    echo "   Waiting... ($i/30)"
    sleep 2
done

# Start the gRPC demo server
echo "🔧 Starting gRPC demo server..."
cd ../Day-1/demos/grpc-demo
docker-compose up -d

# Wait for gRPC server to be ready
echo "⏳ Waiting for gRPC server to be ready..."
sleep 5

echo ""
echo "🎯 Performance Comparison Demo"
echo "=============================="
echo "This demo compares REST API vs gRPC performance:"
echo ""

# Test REST API Performance
echo "🌐 Testing REST API Performance..."
echo "----------------------------------"
echo "1. Testing GET /api/tasks (REST):"
REST_START=$(date +%s%N)
for i in {1..10}; do
    curl -s http://localhost:5000/api/tasks > /dev/null
done
REST_END=$(date +%s%N)
REST_TIME=$((($REST_END - $REST_START) / 1000000))
echo "   ✅ 10 requests completed in ${REST_TIME}ms (avg: $((REST_TIME / 10))ms per request)"
echo ""

echo "2. Testing GET /api/users (REST):"
REST_START=$(date +%s%N)
for i in {1..10}; do
    curl -s http://localhost:5000/api/users > /dev/null
done
REST_END=$(date +%s%N)
REST_TIME=$((($REST_END - $REST_START) / 1000000))
echo "   ✅ 10 requests completed in ${REST_TIME}ms (avg: $((REST_TIME / 10))ms per request)"
echo ""

# Test gRPC Performance
echo "🔧 Testing gRPC Performance..."
echo "------------------------------"
echo "1. Testing GetTasks (gRPC):"
GRPC_START=$(date +%s%N)
for i in {1..10}; do
    python3 client.py get_tasks > /dev/null 2>&1
done
GRPC_END=$(date +%s%N)
GRPC_TIME=$((($GRPC_END - $GRPC_START) / 1000000))
echo "   ✅ 10 requests completed in ${GRPC_TIME}ms (avg: $((GRPC_TIME / 10))ms per request)"
echo ""

echo "2. Testing GetTaskAnalytics (gRPC):"
GRPC_START=$(date +%s%N)
for i in {1..10}; do
    python3 client.py get_analytics > /dev/null 2>&1
done
GRPC_END=$(date +%s%N)
GRPC_TIME=$((($GRPC_END - $GRPC_START) / 1000000))
echo "   ✅ 10 requests completed in ${GRPC_TIME}ms (avg: $((GRPC_TIME / 10))ms per request)"
echo ""

# Performance Comparison
echo "📊 Performance Comparison Summary"
echo "================================="
echo "REST API (HTTP/JSON):"
echo "   - Protocol: HTTP/1.1 with JSON"
echo "   - Serialization: Text-based JSON"
echo "   - Compression: Limited"
echo "   - Use Cases: Web APIs, simple integrations"
echo ""
echo "gRPC (HTTP/2 + Protocol Buffers):"
echo "   - Protocol: HTTP/2 with Protocol Buffers"
echo "   - Serialization: Binary Protocol Buffers"
echo "   - Compression: Built-in compression"
echo "   - Use Cases: Microservices, high-performance APIs"
echo ""
echo "💡 Key Differences:"
echo "   - gRPC typically faster for high-frequency calls"
echo "   - REST more human-readable and debuggable"
echo "   - gRPC better for streaming and bidirectional communication"
echo "   - REST better for simple CRUD operations"
echo ""

echo "🌐 Services Running:"
echo "   - REST API: http://localhost:5000"
echo "   - gRPC Server: localhost:50051"
echo "   - Swagger UI: http://localhost:5000/swagger"
echo ""

echo "🧪 Manual Testing:"
echo "   - REST: curl http://localhost:5000/api/tasks"
echo "   - gRPC: python3 client.py get_tasks"
echo ""

echo "🛑 Press Ctrl+C to stop the demo"

# Keep the demo running
while true; do
    sleep 30
    echo "   Demo still running... (Press Ctrl+C to stop)"
done 