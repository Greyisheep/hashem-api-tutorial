# gRPC Performance Demo - Day 2

## 🎯 Demo Objectives
- Demonstrate gRPC vs REST performance differences
- Show all 4 RPC types in action
- Build gRPC service in .NET
- Understand when to choose gRPC over REST

## ⏰ Demo Timing: 20 minutes

---

## 🚀 Setup Instructions

### Prerequisites
- Docker Desktop running
- .NET 8 SDK installed
- Postman or similar API testing tool

### Quick Start
```bash
# Navigate to Day 1 gRPC demo
cd ../Day-1/demos/grpc-demo

# Start the demo environment
./start-real-demo.sh

# Verify services are running
docker ps
```

---

## 📊 Performance Comparison Demo

### Step 1: REST API Performance Test (5 min)

#### Setup REST Endpoint
```bash
# Start the Python REST API (from Day 1)
cd ../Day-1/demos/grpc-demo
python server.py
```

#### Test REST Performance
```bash
# Using curl to test REST endpoint
curl -X POST http://localhost:8000/analyze \
  -H "Content-Type: application/json" \
  -d '{"task_id": "task_001", "data": "sample task data for analysis"}'
```

**Expected Results:**
- Response time: ~410ms
- Payload size: ~2.3KB
- Content-Type: application/json

#### Performance Analysis
```bash
# Run multiple requests to get average
for i in {1..10}; do
  time curl -X POST http://localhost:8000/analyze \
    -H "Content-Type: application/json" \
    -d '{"task_id": "task_001", "data": "sample task data for analysis"}' \
    -s > /dev/null
done
```

### Step 2: gRPC Performance Test (5 min)

#### Setup gRPC Client
```bash
# Use the existing gRPC client from Day 1
python client.py
```

#### Test gRPC Performance
```python
# The client.py already includes performance testing
# It will show:
# - Unary RPC: ~75ms (82% faster than REST)
# - Server streaming: ~120ms
# - Client streaming: ~95ms
# - Bidirectional streaming: ~150ms
```

**Expected Results:**
- Unary response time: ~75ms
- Payload size: ~1.8KB (22% smaller)
- Protocol: HTTP/2 with Protocol Buffers

### Step 3: Performance Comparison Analysis (5 min)

#### Key Metrics Comparison
| Metric | REST API | gRPC | Improvement |
|--------|----------|------|-------------|
| Response Time | 410ms | 75ms | **82% faster** |
| Payload Size | 2.3KB | 1.8KB | **22% smaller** |
| Protocol | HTTP/1.1 | HTTP/2 | **Multiplexing** |
| Serialization | JSON | Protocol Buffers | **Binary** |

#### Why gRPC is Faster
1. **HTTP/2**: Multiplexing, header compression
2. **Protocol Buffers**: Binary serialization vs text-based JSON
3. **Code Generation**: Optimized client/server code
4. **Streaming**: Built-in support for real-time communication

---

## 🔧 Building gRPC in .NET

### Step 4: .NET gRPC Service Implementation (5 min)

#### Create .NET gRPC Project
```bash
# Create new gRPC project
dotnet new grpc -n TaskFlow.Grpc

# Add to solution
dotnet sln add TaskFlow.Grpc/TaskFlow.Grpc.csproj
```

#### Define Protocol Buffer Schema
```protobuf
// Protos/taskflow.proto
syntax = "proto3";

option csharp_namespace = "TaskFlow.Grpc";

package taskflow;

service TaskFlowService {
  rpc GetTask (GetTaskRequest) returns (TaskResponse);
  rpc CreateTask (CreateTaskRequest) returns (TaskResponse);
  rpc StreamTasks (StreamTasksRequest) returns (stream TaskResponse);
  rpc AnalyzeTask (stream TaskData) returns (AnalysisResponse);
}

message GetTaskRequest {
  string task_id = 1;
}

message CreateTaskRequest {
  string title = 1;
  string description = 2;
  string user_story = 3;
}

message TaskResponse {
  string id = 1;
  string title = 2;
  string description = 3;
  string status = 4;
  string created_at = 5;
  string user_story = 6;
}

message StreamTasksRequest {
  string project_id = 1;
}

message TaskData {
  string task_id = 1;
  string data = 2;
}

message AnalysisResponse {
  string task_id = 1;
  string analysis_result = 2;
  double confidence_score = 3;
}
```

#### Implement gRPC Service
```csharp
// Services/TaskFlowService.cs
using Grpc.Core;
using TaskFlow.Grpc;

namespace TaskFlow.Grpc.Services;

public class TaskFlowService : TaskFlowServiceBase
{
    private readonly ILogger<TaskFlowService> _logger;

    public TaskFlowService(ILogger<TaskFlowService> logger)
    {
        _logger = logger;
    }

    public override async Task<TaskResponse> GetTask(GetTaskRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting task: {TaskId}", request.TaskId);
        
        // Simulate database lookup
        await Task.Delay(50); // Simulate processing time
        
        return new TaskResponse
        {
            Id = request.TaskId,
            Title = "Sample Task",
            Description = "This is a sample task",
            Status = "in_progress",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UserStory = "As a developer, I want to learn gRPC"
        };
    }

    public override async Task<TaskResponse> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating task: {Title}", request.Title);
        
        // Simulate database operation
        await Task.Delay(75); // Simulate processing time
        
        return new TaskResponse
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            Status = "pending",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UserStory = request.UserStory
        };
    }

    public override async Task StreamTasks(StreamTasksRequest request, IServerStreamWriter<TaskResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Streaming tasks for project: {ProjectId}", request.ProjectId);
        
        // Simulate streaming multiple tasks
        for (int i = 1; i <= 5; i++)
        {
            await responseStream.WriteAsync(new TaskResponse
            {
                Id = $"task_{i}",
                Title = $"Task {i}",
                Description = $"Description for task {i}",
                Status = "in_progress",
                CreatedAt = DateTime.UtcNow.ToString("O")
            });
            
            await Task.Delay(200); // Simulate processing time
        }
    }

    public override async Task<AnalysisResponse> AnalyzeTask(IAsyncStreamReader<TaskData> requestStream, ServerCallContext context)
    {
        _logger.LogInformation("Analyzing task data");
        
        var allData = new List<string>();
        
        await foreach (var data in requestStream.ReadAllAsync())
        {
            allData.Add(data.Data);
        }
        
        // Simulate analysis processing
        await Task.Delay(100);
        
        return new AnalysisResponse
        {
            TaskId = "analyzed_task",
            AnalysisResult = $"Analyzed {allData.Count} data chunks",
            ConfidenceScore = 0.95
        };
    }
}
```

#### Configure gRPC in Program.cs
```csharp
// Program.cs
using TaskFlow.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

// Configure gRPC endpoints
app.MapGrpcService<TaskFlowService>();

app.Run();
```

---

## 🧪 Testing the .NET gRPC Service

### Step 5: Performance Testing (5 min)

#### Start the .NET gRPC Service
```bash
cd TaskFlow.Grpc
dotnet run
```

#### Test with gRPC Client
```csharp
// Create a simple test client
using var channel = GrpcChannel.ForAddress("https://localhost:7001");
var client = new TaskFlowService.TaskFlowServiceClient(channel);

// Test unary call
var stopwatch = Stopwatch.StartNew();
var response = await client.GetTaskAsync(new GetTaskRequest { TaskId = "test_001" });
stopwatch.Stop();

Console.WriteLine($"gRPC Response Time: {stopwatch.ElapsedMilliseconds}ms");
```

#### Compare with REST
```bash
# Test equivalent REST endpoint
curl -X GET http://localhost:5000/api/tasks/test_001 \
  -H "Content-Type: application/json"
```

---

## 🎯 Key Learning Points

### When to Use gRPC
1. **Internal Services**: Microservices communication
2. **High Performance**: Low latency requirements
3. **Real-time Streaming**: Live data feeds
4. **Mobile Apps**: Efficient data transfer
5. **Polyglot Environments**: Language-agnostic contracts

### When to Stick with REST
1. **Public APIs**: Better tooling and documentation
2. **Simple CRUD**: Basic data operations
3. **Browser Clients**: Limited gRPC support
4. **Legacy Integration**: Existing REST infrastructure

### Performance Considerations
- **Network**: gRPC shines on high-latency networks
- **Payload Size**: Protocol Buffers are more efficient
- **Connection Reuse**: HTTP/2 multiplexing
- **Code Generation**: Optimized serialization

---

## 🔧 Troubleshooting

### Common Issues
1. **Port Conflicts**: Ensure ports 5001/7001 are available
2. **SSL Certificates**: Use HTTP for local development
3. **Client Generation**: Regenerate client code after proto changes
4. **Streaming Issues**: Check for timeout configurations

### Debug Commands
```bash
# Check if services are running
netstat -an | grep :5001
netstat -an | grep :7001

# Test gRPC reflection
grpcurl -plaintext localhost:5001 list

# Generate client code
protoc --csharp_out=. --grpc_out=. --plugin=protoc-gen-grpc=grpc_csharp_plugin Protos/taskflow.proto
```

---

## 📚 Additional Resources

- [gRPC Official Documentation](https://grpc.io/docs/)
- [.NET gRPC Tutorial](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
- [Protocol Buffers Guide](https://developers.google.com/protocol-buffers)
- [gRPC vs REST Performance](https://grpc.io/blog/grpc-stacks/)

---

**Demo Success**: Students understand the performance benefits of gRPC and can implement gRPC services in .NET for appropriate use cases. 