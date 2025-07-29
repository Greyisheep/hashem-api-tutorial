# C# Guide for TaskFlow API - A Beginner's Journey

Welcome to C#! This guide will walk you through the TaskFlow API codebase and explain the C# concepts, patterns, and architecture used. Since this is your first time with C#, we'll start with the basics and build up to more advanced concepts.

## 🎯 Table of Contents

1. [C# Basics & Syntax](#1-c-basics--syntax)
2. [Project Structure Overview](#2-project-structure-overview)
3. [Domain-Driven Design (DDD) Explained](#3-domain-driven-design-ddd-explained)
4. [Key C# Patterns Used](#4-key-c-patterns-used)
5. [Code Architecture Deep Dive](#5-code-architecture-deep-dive)
6. [Security Implementation](#6-security-implementation)
7. [Testing and Quality](#7-testing-and-quality)
8. [Production Considerations](#8-production-considerations)

---

## 1. C# Basics & Syntax

### What is C#?
C# (pronounced "C-Sharp") is a modern, object-oriented programming language developed by Microsoft. It's similar to Java but with more modern features and excellent tooling.

### Key C# Concepts You'll See

#### 1. Namespaces
```csharp
namespace TaskFlow.API.Controllers;  // Organizes code into logical groups
```
Think of namespaces like folders for your code. They prevent naming conflicts and organize functionality.

#### 2. Classes and Records
```csharp
// Traditional class (mutable)
public class User
{
    public string Name { get; set; }  // Property with getter and setter
}

// Record (immutable, great for DTOs)
public record CreateTaskRequest
{
    public string Title { get; init; } = string.Empty;  // init = set only during creation
}
```

#### 3. Properties vs Fields
```csharp
public class Example
{
    private string _field;           // Field (private storage)
    public string Property { get; set; }  // Property (public access with logic)
    public string ReadOnly { get; }        // Read-only property
}
```

#### 4. Nullable Reference Types
```csharp
public string Name { get; set; }     // Cannot be null
public string? Description { get; set; }  // Can be null (? makes it nullable)
```

## 2. Project Structure Overview

Our TaskFlow API follows **Clean Architecture** principles:

```
TaskFlow.API/
├── src/
│   ├── TaskFlow.API/              # 🌐 Web API Layer (Controllers, Startup)
│   ├── TaskFlow.Application/      # 📋 Business Logic & Use Cases
│   ├── TaskFlow.Domain/           # 🏢 Core Business Rules & Entities
│   └── TaskFlow.Infrastructure/   # 🔧 Data Access & External Services
```

### Layer Responsibilities

#### 🌐 **API Layer** (`TaskFlow.API`)
- **Controllers**: Handle HTTP requests/responses
- **Models**: API-specific data structures
- **Program.cs**: Application startup and configuration

```csharp
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    // Handles HTTP requests and delegates to Application layer
}
```

#### 📋 **Application Layer** (`TaskFlow.Application`)
- **Commands**: Operations that change data (Create, Update, Delete)
- **Queries**: Operations that read data (Get, List)
- **DTOs**: Data Transfer Objects for moving data between layers
- **Interfaces**: Contracts for the Infrastructure layer

```csharp
// Command for creating a task
public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

// Handler that contains the business logic
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    // Business logic here
}
```

#### 🏢 **Domain Layer** (`TaskFlow.Domain`)
- **Entities**: Core business objects with behavior
- **Value Objects**: Immutable objects that represent values
- **Domain Events**: Things that happen in the business
- **Exceptions**: Business rule violations

```csharp
// Entity - has identity and behavior
public class Task : AggregateRoot
{
    public TaskId Id { get; private set; }           // Identity
    public TaskTitle Title { get; private set; }     // Value Object
    
    // Business method
    public void AssignTo(string assigneeId)
    {
        // Business rules here
        if (string.IsNullOrWhiteSpace(assigneeId))
            throw new DomainException("Assignee ID cannot be empty");
            
        AssigneeId = assigneeId;
        AddDomainEvent(new TaskAssignedEvent(Id.Value, assigneeId));
    }
}
```

#### 🔧 **Infrastructure Layer** (`TaskFlow.Infrastructure`)
- **Repositories**: Data access implementations
- **DbContext**: Entity Framework database context
- **Configurations**: Database mappings

```csharp
// Repository implementation
public class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _context;
    
    public async Task<Domain.Entities.Task?> GetByIdAsync(string id)
    {
        return await _context.Tasks.FindAsync(id);
    }
}
```

## 3. Domain-Driven Design (DDD) Explained

### What is DDD?
Domain-Driven Design is an approach to software development that focuses on the **business domain** - the actual problem you're solving.

### Key DDD Concepts in Our Code

#### **Entities** (Objects with Identity)
```csharp
public class Task : AggregateRoot  // Has unique identity
{
    public TaskId Id { get; private set; }  // Unique identifier
    
    // Constructor enforces business rules
    public Task(TaskTitle title, string description, ProjectId projectId, string createdBy)
    {
        Id = TaskId.New();  // Generate unique ID
        Title = title ?? throw new DomainException("Task title cannot be null");
        // ... more validation
    }
}
```

#### **Value Objects** (Objects without Identity)
```csharp
public class TaskTitle : ValueObject
{
    public string Value { get; }
    
    private TaskTitle(string value)
    {
        Value = value;
    }
    
    public static TaskTitle From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Task title cannot be empty");
        if (value.Length > 200)
            throw new DomainException("Task title cannot exceed 200 characters");
            
        return new TaskTitle(value.Trim());
    }
}
```

#### **Domain Events** (Things That Happen)
```csharp
public record TaskCreatedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    
    public TaskCreatedEvent(string taskId, string title)
    {
        TaskId = taskId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}
```

## 4. Key C# Patterns Used

### **1. CQRS (Command Query Responsibility Segregation)**

We separate operations that **change** data from operations that **read** data:

```csharp
// Command (Changes data)
public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;
}

// Query (Reads data)
public record GetTaskQuery(string Id) : IRequest<TaskDto?>;
```

### **2. Mediator Pattern**

Mediator handles communication between objects without them knowing about each other:

```csharp
[ApiController]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public async Task<ActionResult> CreateTask(CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);  // Mediator finds the handler
        return Ok(result);
    }
}
```

### **3. Repository Pattern**

Repository provides a collection-like interface for data access:

```csharp
public interface ITaskRepository
{
    Task<Domain.Entities.Task?> GetByIdAsync(string id);
    Task<Domain.Entities.Task> AddAsync(Domain.Entities.Task task);
}
```

### **4. Dependency Injection**

Objects don't create their dependencies; they receive them:

```csharp
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;  // Dependency
    
    // Constructor injection
    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }
}

// Registration in Program.cs
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
```

### **5. Result/Response Wrapper Pattern**

Consistent API responses:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
}

// Usage
return Ok(ApiResponse<TaskDto>.SuccessResponse(task, "Task created successfully"));
```

## 5. Code Architecture Deep Dive

### **Startup & Configuration (Program.cs)**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();                    // MVC Controllers
builder.Services.AddDbContext<TaskFlowDbContext>();   // Database
builder.Services.AddMediatR();                        // CQRS/Mediator
builder.Services.AddAuthentication();                 // JWT Auth
builder.Services.AddSwaggerGen();                     // API Documentation

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseAuthentication();  // Check who you are
app.UseAuthorization();   // Check what you can do
app.MapControllers();     // Route to controllers

app.Run();
```

### **Entity Framework & Database**

```csharp
public class TaskFlowDbContext : DbContext, IUnitOfWork
{
    public DbSet<Domain.Entities.Task> Tasks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
    }
}

// Configuration for Task entity
public class TaskConfiguration : IEntityTypeConfiguration<Domain.Entities.Task>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Task> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title)
            .HasConversion(
                title => title.Value,           // To database
                value => TaskTitle.From(value)  // From database
            );
    }
}
```

### **Authentication & Authorization**

```csharp
// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Usage in controller
[Authorize]  // Requires authentication
[HttpGet]
public async Task<ActionResult> GetTasks()
{
    var userId = User.Identity?.Name;  // Get current user
    // ...
}
```

## 6. Security Implementation

### **JWT Token Generation**

```csharp
private string GenerateJwtToken(string username, string role)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey), 
            SecurityAlgorithms.HmacSha256Signature)
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

### **Security Headers**

```csharp
// Security middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});
```

### **Input Validation**

```csharp
// Model validation
public record CreateTaskCommand : IRequest<TaskDto>
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; init; } = string.Empty;
}

// Business rule validation in domain
public void UpdateStatus(TaskState newStatus)
{
    if (newStatus == null)
        throw new DomainException("Status cannot be null");
    if (Status == TaskState.Completed && newStatus != TaskState.Completed)
        throw new DomainException("Cannot change status of completed task");
}
```

## 7. Testing and Quality

### **Unit Test Example**

```csharp
[Test]
public void Task_Creation_Should_Generate_TaskCreatedEvent()
{
    // Arrange
    var title = TaskTitle.From("Test Task");
    var projectId = ProjectId.From("project_123");
    
    // Act
    var task = new Domain.Entities.Task(title, "Description", projectId, "user_123");
    
    // Assert
    Assert.That(task.DomainEvents, Has.Count.EqualTo(1));
    Assert.That(task.DomainEvents.First(), Is.TypeOf<TaskCreatedEvent>());
}
```

### **Integration Test Example**

```csharp
[Test]
public async Task CreateTask_ShouldReturnCreatedTask()
{
    // Arrange
    var client = _factory.CreateClient();
    var command = new CreateTaskCommand
    {
        Title = "Integration Test Task",
        Description = "Testing the API"
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/tasks", command);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TaskDto>>();
    Assert.That(result.Success, Is.True);
    Assert.That(result.Data.Title, Is.EqualTo("Integration Test Task"));
}
```

## 8. Production Considerations

### **Logging with Serilog**

```csharp
// Structured logging configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", environment)
    .WriteTo.Console()
    .WriteTo.File("logs/taskflow-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://seq:5341")  // Centralized logging
    .CreateLogger();

// Usage in controllers
_logger.LogInformation("Creating task: {Title} for user: {UserId}", 
    request.Title, User.Identity?.Name);
```

### **Health Checks**

```csharp
// Health check configuration
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskFlowDbContext>("Database")
    .AddCheck("Self", () => HealthCheckResult.Healthy("API is healthy"));

// Custom health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});
```

### **Docker Configuration**

```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "TaskFlow.sln" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

## 🎓 Key Takeaways for C# Beginners

1. **C# is Strongly Typed**: Everything has a type, and the compiler checks them
2. **Null Safety**: Use `?` for nullable types, avoid null reference exceptions
3. **Properties over Fields**: Use public properties for clean APIs
4. **Dependency Injection**: Don't create dependencies, inject them
5. **Async/Await**: Use for I/O operations (database, HTTP calls)
6. **Records for Data**: Use records for immutable data transfer objects
7. **Patterns Matter**: CQRS, Repository, and DDD patterns organize complex code
8. **Security First**: Always validate input, use HTTPS, implement proper auth

## 🚀 Next Steps

1. **Explore the Swagger UI**: Visit `http://localhost:5000/swagger` to see all APIs
2. **Check the Logs**: Use `docker-compose logs taskflow-api` to see application logs
3. **Test the APIs**: Use the Postman collection in `/postman` folder
4. **Modify Code**: Try adding a new endpoint or changing business logic
5. **Run Tests**: Execute `dotnet test` to run the test suite

Welcome to the wonderful world of C# and .NET! 🎉

---

*This guide covers the fundamentals. For advanced topics like event sourcing, microservices, and performance optimization, refer to the official .NET documentation and community resources.* 