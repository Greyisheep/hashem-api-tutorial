# Day 2: API Implementation & Advanced Patterns

## 🎯 Day 2 Objectives
By 4:00 PM, students should be able to:
- [ ] Review and critique C#/.NET API implementations from take-home
- [ ] Understand gRPC vs REST performance implications through live demo
- [ ] Design and implement GraphQL schemas for complex data requirements
- [ ] Structure .NET APIs following Domain-Driven Design principles
- [ ] Implement comprehensive logging and monitoring
- [ ] Design API versioning strategies
- [ ] Apply security best practices in .NET APIs
- [ ] Set up automated testing and CI/CD pipelines

**Key Deliverable**: Production-ready .NET TaskFlow API with gRPC services, GraphQL endpoint, and security foundations.

---

## ⏰ Detailed Schedule

### 10:00-10:30 AM: Take-Home Review & C# Showcase (30 min)
**Activity**: Students present their .NET TaskFlow implementations (3 min each)
**Materials**: [`exercises/take-home-review-template.md`](./exercises/take-home-review-template.md)

### 10:30-10:45 AM: Break (15 min)

### 10:45-11:30 AM: Module 2A - gRPC Deep Dive & Performance (45 min)
**Demo**: Live gRPC vs REST performance comparison
**Materials**: [`demos/grpc-performance-demo.md`](./demos/grpc-performance-demo.md)

### 11:30-12:00 PM: Module 2B - GraphQL Implementation (30 min)
**Demo**: GraphQL vs REST for complex data requirements
**Materials**: [`demos/graphql-implementation-demo.md`](./demos/graphql-implementation-demo.md)

### 12:00-1:00 PM: Lunch (60 min)

### 1:00-2:00 PM: Module 2C - Domain-Driven Design in .NET (60 min)
**Implementation**: Build DDD structure for TaskFlow
**Materials**: [`implementation/ddd-structure-guide.md`](./implementation/ddd-structure-guide.md)

### 2:00-2:15 PM: Break (15 min)

### 2:15-3:00 PM: Module 2D - Logging & Monitoring (45 min)
**Implementation**: Structured logging and health checks
**Materials**: [`implementation/logging-monitoring-guide.md`](./implementation/logging-monitoring-guide.md)

### 3:00-3:15 PM: Break (15 min)

### 3:15-4:00 PM: Module 2E - API Versioning & Documentation (45 min)
**Implementation**: Versioning strategies and OpenAPI documentation
**Materials**: [`implementation/api-versioning-documentation-guide.md`](./implementation/api-versioning-documentation-guide.md)

### 4:00-4:15 PM: Break (15 min)

### 4:15-5:00 PM: Module 2F - Security Foundations (45 min)
**Implementation**: JWT authentication and security best practices
**Materials**: [`implementation/security-foundations-guide.md`](./implementation/security-foundations-guide.md)

---

## 🛠️ Technical Setup Requirements

### Prerequisites
- [ ] .NET 8 SDK installed
- [ ] Visual Studio 2022 or VS Code with C# extensions
- [ ] Docker Desktop running
- [ ] Postman or similar API testing tool
- [ ] Git and GitHub account

### Required Packages
```bash
# gRPC packages
dotnet add package Grpc.AspNetCore
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools

# GraphQL packages
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data.EntityFramework

# Authentication packages
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package System.IdentityModel.Tokens.Jwt

# Logging packages
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# API versioning packages
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer

# Testing packages
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit
dotnet add package FluentAssertions
dotnet add package Moq
```

---

## 📁 Project Structure

```
Day-2/
├── facilitator-guide.md                    # Complete facilitation guide
├── README.md                               # This file
├── demos/                                  # Demo materials
│   ├── grpc-performance-demo.md           # gRPC performance comparison
│   └── graphql-implementation-demo.md     # GraphQL implementation
├── implementation/                         # Implementation guides
│   ├── ddd-structure-guide.md             # DDD implementation
│   ├── logging-monitoring-guide.md        # Logging and monitoring
│   ├── api-versioning-documentation-guide.md # API versioning
│   └── security-foundations-guide.md      # Security implementation
├── exercises/                              # Student exercises
│   └── take-home-review-template.md       # Take-home review template
├── code-along/                             # Code-along materials
├── tests/                                  # Testing materials
└── workshops/                              # Workshop materials
```

---

## 🎯 Learning Outcomes

### Technical Skills
- **gRPC Implementation**: Build high-performance gRPC services in .NET
- **GraphQL Design**: Create GraphQL schemas and resolvers
- **DDD Architecture**: Implement clean architecture with domain-driven design
- **Security Implementation**: JWT authentication and authorization
- **API Versioning**: Multiple versioning strategies for API evolution
- **Monitoring & Logging**: Production-ready observability patterns

### Production Readiness
- **Performance Optimization**: gRPC vs REST decision framework
- **Error Handling**: Comprehensive error management strategies
- **Documentation**: Professional API documentation with OpenAPI
- **Testing**: Unit and integration testing patterns
- **Security**: OWASP Top 10 awareness and implementation

### Best Practices
- **Code Organization**: Clean architecture and separation of concerns
- **API Design**: RESTful and GraphQL design patterns
- **Data Validation**: Input validation and sanitization
- **Logging**: Structured logging with correlation IDs
- **Monitoring**: Health checks and metrics collection

---

## 🔧 Implementation Examples

### gRPC Service Example
```csharp
// Protos/taskflow.proto
service TaskFlowService {
  rpc GetTask (GetTaskRequest) returns (TaskResponse);
  rpc CreateTask (CreateTaskRequest) returns (TaskResponse);
  rpc StreamTasks (StreamTasksRequest) returns (stream TaskResponse);
}

// Services/TaskFlowService.cs
public class TaskFlowService : TaskFlowServiceBase
{
    public override async Task<TaskResponse> GetTask(GetTaskRequest request, ServerCallContext context)
    {
        // Implementation with performance monitoring
        var stopwatch = Stopwatch.StartNew();
        var result = await _taskRepository.GetByIdAsync(TaskId.From(request.TaskId));
        stopwatch.Stop();
        
        _logger.LogInformation("gRPC GetTask completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
        return result?.ToGrpcResponse() ?? new TaskResponse();
    }
}
```

### GraphQL Schema Example
```csharp
// GraphQL/Types/TaskType.cs
public class TaskType : ObjectType<Task>
{
    protected override void Configure(IObjectTypeDescriptor<Task> descriptor)
    {
        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Title).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.Assignee).ResolveWith<TaskResolvers>(t => t.GetAssignee(default!, default!));
        descriptor.Field(t => t.Comments).ResolveWith<TaskResolvers>(t => t.GetComments(default!, default!));
    }
}

// GraphQL/Queries/Query.cs
public class Query
{
    public async Task<Task?> GetTask(string id, [Service] ITaskService taskService)
    {
        return await taskService.GetTaskAsync(TaskId.From(id));
    }
}
```

### DDD Domain Model Example
```csharp
// Domain/Entities/Task.cs
public class Task : AggregateRoot
{
    public TaskId Id { get; private set; }
    public TaskTitle Title { get; private set; }
    public TaskStatus Status { get; private set; }
    
    public Task(TaskTitle title, TaskDescription description, ProjectId projectId)
    {
        Id = TaskId.New();
        Title = title ?? throw new DomainException("Task title cannot be null");
        Status = TaskStatus.Pending;
        
        AddDomainEvent(new TaskCreatedEvent(Id, Title.Value));
    }
    
    public void AssignTo(UserId assigneeId)
    {
        if (Status == TaskStatus.Completed)
            throw new DomainException("Cannot assign completed tasks");
            
        AssigneeId = assigneeId;
        Status = TaskStatus.InProgress;
        AddDomainEvent(new TaskAssignedEvent(Id, assigneeId.Value));
    }
}
```

### Security Implementation Example
```csharp
// Controllers/TasksController.cs
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    [HttpPost]
    [RequirePermission("task:create")]
    [ProducesResponseType(typeof(CreateTaskResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateTaskResponse>> CreateTask(CreateTaskCommand command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        command.CreatedBy = userId;
        
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = result.TaskId }, result);
    }
}
```

---

## 📊 Assessment Criteria

### End of Day Checklist
**Each student should have**:
- [ ] Working .NET TaskFlow API with DDD structure
- [ ] gRPC service implementation
- [ ] GraphQL endpoint
- [ ] Comprehensive logging setup
- [ ] API versioning strategy
- [ ] Security foundations
- [ ] Automated tests
- [ ] API documentation

### Success Metrics
- **Technical Implementation**: Clean, production-ready code
- **Architecture Understanding**: Proper DDD and clean architecture
- **Security Awareness**: Authentication and authorization implementation
- **Performance Knowledge**: gRPC vs REST decision making
- **Documentation Quality**: Professional API documentation

---

## 🚀 Next Steps

### Day 3 Preparation
- [ ] Review security implementation
- [ ] Prepare for advanced security topics
- [ ] Understand production deployment requirements
- [ ] Foundation for enterprise security patterns

### Portfolio Enhancement
- [ ] Complete TaskFlow API implementation
- [ ] Add gRPC and GraphQL services
- [ ] Implement comprehensive testing
- [ ] Create professional documentation
- [ ] Deploy to cloud platform

---

## 📚 Additional Resources

### Documentation
- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [gRPC Documentation](https://grpc.io/docs/)
- [GraphQL Documentation](https://graphql.org/learn/)
- [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate)

### Tutorials & Guides
- [.NET gRPC Tutorial](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
- [GraphQL in .NET](https://chillicream.com/docs/hotchocolate/get-started)
- [DDD in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [JWT Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

### Tools & Extensions
- [Visual Studio](https://visualstudio.microsoft.com/)
- [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/)
- [GraphQL Playground](https://github.com/graphql/graphql-playground)

---

## 🎪 Instructor Notes

### Key Teaching Points
1. **Performance First**: gRPC demo creates immediate impact
2. **Hands-on Focus**: Keep students coding throughout the day
3. **Production Mindset**: Emphasize real-world applicability
4. **Security Awareness**: Build security consciousness from day one

### Common Challenges
1. **gRPC Setup**: Have backup demo ready
2. **GraphQL Complexity**: Start simple, build complexity gradually
3. **DDD Overwhelm**: Focus on practical patterns, not theory
4. **Time Management**: Prioritize hands-on over lectures

### Success Indicators
- Students excited about their implementations
- Clear understanding of when to use different technologies
- Production-ready code quality
- Confidence in security implementation

---

**Day 2 Success**: Students leave with a complete, production-ready .NET API and the confidence to implement enterprise-grade solutions with advanced patterns like gRPC and GraphQL. 