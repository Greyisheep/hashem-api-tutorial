#!/bin/bash

# GraphQL Implementation Demo Script
# This script sets up and runs the GraphQL demo for Day 2

set -e

echo "🚀 Starting GraphQL Implementation Demo..."
echo "=========================================="

# Check prerequisites
echo "📋 Checking prerequisites..."

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 8.0 SDK first."
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

echo "✅ Prerequisites check passed"

# Create GraphQL project if it doesn't exist
GRAPHQL_PROJECT_DIR="TaskFlow.GraphQL"
if [ ! -d "$GRAPHQL_PROJECT_DIR" ]; then
    echo "📁 Creating GraphQL project..."
    dotnet new web -n TaskFlow.GraphQL
    cd TaskFlow.GraphQL
    
    # Add required packages
    echo "📦 Adding GraphQL packages..."
    dotnet add package HotChocolate.AspNetCore
    dotnet add package HotChocolate.Data.EntityFramework
    dotnet add package Microsoft.EntityFrameworkCore.InMemory
    
    # Create basic GraphQL schema
    echo "🔧 Creating GraphQL schema..."
    mkdir -p GraphQL
    cat > GraphQL/Query.cs << 'EOF'
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskFlow.GraphQL.GraphQL
{
    public class Query
    {
        [GraphQLDescription("Get all tasks")]
        public async Task<List<TaskDto>> GetTasks([Service] ITaskService taskService)
        {
            return await taskService.GetAllTasksAsync();
        }

        [GraphQLDescription("Get task by ID")]
        public async Task<TaskDto?> GetTask(string id, [Service] ITaskService taskService)
        {
            return await taskService.GetTaskByIdAsync(id);
        }
    }

    public class TaskDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
    }

    public interface ITaskService
    {
        Task<List<TaskDto>> GetAllTasksAsync();
        Task<TaskDto?> GetTaskByIdAsync(string id);
    }

    public class TaskService : ITaskService
    {
        public async Task<List<TaskDto>> GetAllTasksAsync()
        {
            // Demo data - in production this would call the REST API
            return new List<TaskDto>
            {
                new TaskDto { Id = "1", Title = "Implement Authentication", Description = "JWT auth", Status = "pending", ProjectId = "1" },
                new TaskDto { Id = "2", Title = "Design Database", Description = "DDD schema", Status = "completed", ProjectId = "1" }
            };
        }

        public async Task<TaskDto?> GetTaskByIdAsync(string id)
        {
            var tasks = await GetAllTasksAsync();
            return tasks.FirstOrDefault(t => t.Id == id);
        }
    }
}
EOF

    # Update Program.cs
    cat > Program.cs << 'EOF'
using HotChocolate.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<GraphQL.Query>()
    .AddType<TaskDto>()
    .AddFiltering()
    .AddSorting();

var app = builder.Build();

app.MapGraphQL();

app.Run();
EOF

    cd ..
    echo "✅ GraphQL project created"
else
    echo "✅ GraphQL project already exists"
fi

# Start the main TaskFlow API
echo "🌐 Starting TaskFlow API..."
cd ../taskflow-api-dotnet
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

# Start GraphQL server
echo "🔮 Starting GraphQL server..."
cd ../Day-2/demos/graphql-demo
cd TaskFlow.GraphQL
dotnet run --urls "http://localhost:5001" &

GRAPHQL_PID=$!

echo "✅ GraphQL server started on http://localhost:5001/graphql"
echo ""
echo "🎯 Demo Setup Complete!"
echo "======================="
echo "📊 REST API: http://localhost:5000/swagger"
echo "🔮 GraphQL: http://localhost:5001/graphql"
echo "📈 Health Check: http://localhost:5000/health"
echo ""
echo "💡 Try these GraphQL queries:"
echo "   - Get all tasks: { tasks { id title status } }"
echo "   - Get specific task: { task(id: \"1\") { id title description status } }"
echo ""
echo "🛑 Press Ctrl+C to stop the demo"

# Wait for user to stop
wait $GRAPHQL_PID 