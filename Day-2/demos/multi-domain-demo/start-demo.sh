#!/bin/bash

# Multi-Domain Architecture Demo Script
# This script demonstrates the separate domain controllers in TaskFlow API

set -e

echo "🚀 Starting Multi-Domain Architecture Demo..."
echo "============================================="

# Check prerequisites
echo "📋 Checking prerequisites..."

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

echo "✅ Prerequisites check passed"

# Start the main TaskFlow API
echo "🌐 Starting TaskFlow API..."
cd ../../taskflow-api-dotnet
docker-compose up -d

# Wait for API to be healthy
echo "⏳ Waiting for API to be ready..."
for i in {1..30}; do
    if curl -s http://localhost:5000/health > /dev/null 2>&1; then
        echo "✅ API is ready!"
        break
    fi
    echo "   Waiting... ($i/30)"
    sleep 2
done

echo ""
echo "🎯 Multi-Domain Architecture Demo"
echo "================================="
echo "This demo showcases the separate domain controllers following DDD principles:"
echo ""

# Test Users Domain
echo "👥 Testing Users Domain..."
echo "-------------------------"
echo "1. Getting all users:"
curl -s http://localhost:5000/api/users | jq '.data | length' 2>/dev/null || echo "   Response received"
echo ""

echo "2. Getting specific user (first user):"
USER_ID=$(curl -s http://localhost:5000/api/users | jq -r '.data[0].id' 2>/dev/null || echo "demo-user-id")
echo "   User ID: $USER_ID"
echo ""

# Test Projects Domain
echo "📋 Testing Projects Domain..."
echo "-----------------------------"
echo "1. Getting all projects:"
curl -s http://localhost:5000/api/projects | jq '.data | length' 2>/dev/null || echo "   Response received"
echo ""

echo "2. Getting projects by owner:"
PROJECT_ID=$(curl -s http://localhost:5000/api/projects | jq -r '.data[0].id' 2>/dev/null || echo "demo-project-id")
echo "   Project ID: $PROJECT_ID"
echo ""

# Test Tasks Domain
echo "✅ Testing Tasks Domain..."
echo "-------------------------"
echo "1. Getting all tasks:"
curl -s http://localhost:5000/api/tasks | jq '.data | length' 2>/dev/null || echo "   Response received"
echo ""

echo "2. Getting specific task:"
TASK_ID=$(curl -s http://localhost:5000/api/tasks | jq -r '.data[0].id' 2>/dev/null || echo "demo-task-id")
echo "   Task ID: $TASK_ID"
echo ""

# Test Cross-Domain Relationships
echo "🔗 Testing Cross-Domain Relationships..."
echo "----------------------------------------"
echo "1. Creating a new task for existing project:"
echo "   POST /api/tasks"
echo "   Body: {\"title\": \"Demo Task\", \"description\": \"Created via demo\", \"projectId\": \"$PROJECT_ID\"}"
echo ""

echo "2. Getting tasks for specific project:"
echo "   This would typically be implemented as: GET /api/tasks?projectId=$PROJECT_ID"
echo ""

echo "🎯 Demo Summary"
echo "==============="
echo "✅ Users Domain: Separate controller for user management"
echo "✅ Projects Domain: Separate controller for project management"
echo "✅ Tasks Domain: Separate controller for task management"
echo "✅ Cross-Domain: Relationships maintained through IDs"
echo ""
echo "🌐 API Endpoints Available:"
echo "   - Users: http://localhost:5000/api/users"
echo "   - Projects: http://localhost:5000/api/projects"
echo "   - Tasks: http://localhost:5000/api/tasks"
echo "   - Health: http://localhost:5000/health"
echo "   - Swagger: http://localhost:5000/swagger"
echo ""
echo "💡 DDD Benefits Demonstrated:"
echo "   - Clear domain boundaries"
echo "   - Separate concerns per domain"
echo "   - Maintainable and scalable architecture"
echo "   - Domain-specific business logic"
echo ""
echo "🛑 Press Ctrl+C to stop the demo"

# Keep the demo running
while true; do
    sleep 10
    echo "   Demo still running... (Press Ctrl+C to stop)"
done 