# Logging & Monitoring Implementation Guide - .NET

## 🎯 Implementation Objectives
- Implement structured logging with Serilog
- Add comprehensive health checks
- Set up application monitoring and metrics
- Create observability patterns for production debugging

## ⏰ Implementation Timing: 45 minutes

---

## 📝 Structured Logging Implementation

### Step 1: Serilog Configuration

#### Install Required Packages
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
dotnet add package Serilog.Enrichers.Process
```

#### Program.cs Configuration
```csharp
// Program.cs
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/taskflow-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

try
{
    Log.Information("Starting TaskFlow API");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog for logging
    builder.Host.UseSerilog();
    
    // ... rest of configuration
    
    var app = builder.Build();
    
    // ... middleware configuration
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### Step 2: Logging Middleware

#### Request Logging Middleware
```csharp
// TaskFlow.API/Middleware/RequestLoggingMiddleware.cs
namespace TaskFlow.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Items["CorrelationId"] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
            ["ClientIP"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        });

        var sw = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting request {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            await _next(context);

            sw.Stop();
            
            _logger.LogInformation("Completed request {Method} {Path} in {ElapsedMs}ms with status {StatusCode}", 
                context.Request.Method, 
                context.Request.Path, 
                sw.ElapsedMilliseconds, 
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            _logger.LogError(ex, "Request {Method} {Path} failed after {ElapsedMs}ms", 
                context.Request.Method, 
                context.Request.Path, 
                sw.ElapsedMilliseconds);
            
            throw;
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string CorrelationIdHeader = "X-Correlation-ID";
        
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            return correlationId.ToString();
        }

        var newCorrelationId = Guid.NewGuid().ToString();
        context.Request.Headers[CorrelationIdHeader] = newCorrelationId;
        return newCorrelationId;
    }
}
```

#### Exception Handling Middleware
```csharp
// TaskFlow.API/Middleware/ExceptionHandlingMiddleware.cs
namespace TaskFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
        
        _logger.LogError(exception, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            DomainException => StatusCodes.Status400BadRequest,
            ApplicationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            success = false,
            error = _environment.IsDevelopment() ? exception.GetType().Name : "InternalServerError",
            message = _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request.",
            correlationId = correlationId,
            timestamp = DateTime.UtcNow.ToString("O")
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### Step 3: Enhanced Controller Logging

#### TasksController with Logging
```csharp
// TaskFlow.API/Controllers/TasksController.cs
namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTaskResponse>> CreateTask(CreateTaskCommand command)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "CreateTask",
            ["ProjectId"] = command.ProjectId,
            ["AssigneeId"] = command.AssigneeId ?? "unassigned"
        });

        _logger.LogInformation("Creating new task: {Title}", command.Title);

        try
        {
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Task created successfully. TaskId: {TaskId}", result.TaskId);
            
            return CreatedAtAction(nameof(GetTask), new { id = result.TaskId }, result);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error creating task: {Title}", command.Title);
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error creating task: {Title}", command.Title);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating task: {Title}", command.Title);
            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTaskResponse>> GetTask(string id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetTask",
            ["TaskId"] = id
        });

        _logger.LogInformation("Retrieving task: {TaskId}", id);

        var query = new GetTaskQuery { TaskId = id };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", id);
            return NotFound();
        }

        _logger.LogInformation("Task retrieved successfully: {TaskId}", id);
        return Ok(result);
    }
}
```

---

## 🏥 Health Checks Implementation

### Step 4: Health Check Configuration

#### Install Health Check Packages
```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.Redis
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.UrlGroup
```

#### Program.cs Health Check Configuration
```csharp
// Program.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskFlowDbContext>("Database")
    .AddRedis("localhost:6379", name: "Redis")
    .AddUrlGroup(new Uri("https://api.external-service.com/health"), name: "External API")
    .AddCheck("Self", () => HealthCheckResult.Healthy("API is healthy"))
    .AddCheck("Configuration", () => 
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        return !string.IsNullOrEmpty(connectionString) 
            ? HealthCheckResult.Healthy("Configuration is valid")
            : HealthCheckResult.Unhealthy("Configuration is missing");
    });

var app = builder.Build();

// Configure health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow.ToString("O"),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

### Step 5: Custom Health Checks

#### Database Health Check
```csharp
// TaskFlow.Infrastructure/HealthChecks/DatabaseHealthCheck.cs
namespace TaskFlow.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly TaskFlowDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(TaskFlowDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connectivity
            await _context.Database.CanConnectAsync(cancellationToken);
            
            // Test a simple query
            var taskCount = await _context.Tasks.CountAsync(cancellationToken);
            
            _logger.LogDebug("Database health check passed. Task count: {TaskCount}", taskCount);
            
            return HealthCheckResult.Healthy("Database is accessible", new Dictionary<string, object>
            {
                ["TaskCount"] = taskCount,
                ["ConnectionString"] = _context.Database.GetConnectionString() ?? "unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}
```

#### External Service Health Check
```csharp
// TaskFlow.Infrastructure/HealthChecks/ExternalServiceHealthCheck.cs
namespace TaskFlow.Infrastructure.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly string _serviceUrl;

    public ExternalServiceHealthCheck(
        HttpClient httpClient, 
        ILogger<ExternalServiceHealthCheck> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrl = configuration["ExternalService:HealthUrl"] ?? "https://api.external-service.com/health";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_serviceUrl, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("External service health check passed");
                return HealthCheckResult.Healthy("External service is accessible");
            }
            
            _logger.LogWarning("External service health check failed with status: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded("External service is responding but not healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service health check failed");
            return HealthCheckResult.Unhealthy("External service is not accessible", ex);
        }
    }
}
```

---

## 📊 Application Monitoring & Metrics

### Step 6: Metrics Implementation

#### Install Metrics Packages
```bash
dotnet add package PrometheusNet
dotnet add package PrometheusNet.AspNetCore
dotnet add package PrometheusNet.AspNetCore.HealthChecks
```

#### Metrics Configuration
```csharp
// TaskFlow.API/Metrics/ApplicationMetrics.cs
namespace TaskFlow.API.Metrics;

public static class ApplicationMetrics
{
    public static readonly Counter RequestCounter = Metrics
        .CreateCounter("taskflow_requests_total", "Total number of requests", 
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });

    public static readonly Histogram RequestDuration = Metrics
        .CreateHistogram("taskflow_request_duration_seconds", "Request duration in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });

    public static readonly Counter TaskCreatedCounter = Metrics
        .CreateCounter("taskflow_tasks_created_total", "Total number of tasks created");

    public static readonly Gauge ActiveTasksGauge = Metrics
        .CreateGauge("taskflow_active_tasks", "Number of active tasks");

    public static readonly Histogram DatabaseQueryDuration = Metrics
        .CreateHistogram("taskflow_database_query_duration_seconds", "Database query duration in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "operation", "table" },
                Buckets = new[] { 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1 }
            });
}
```

#### Metrics Middleware
```csharp
// TaskFlow.API/Middleware/MetricsMiddleware.cs
namespace TaskFlow.API.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var method = context.Request.Method;
        var endpoint = context.Request.Path.Value ?? "/";

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode.ToString();

            // Record metrics
            ApplicationMetrics.RequestCounter
                .WithLabels(method, endpoint, statusCode)
                .Inc();

            ApplicationMetrics.RequestDuration
                .WithLabels(method, endpoint)
                .Observe(sw.Elapsed.TotalSeconds);

            _logger.LogDebug("Request {Method} {Endpoint} completed in {Duration}ms with status {StatusCode}",
                method, endpoint, sw.ElapsedMilliseconds, statusCode);
        }
    }
}
```

#### Program.cs Metrics Configuration
```csharp
// Program.cs
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// ... other configuration

var app = builder.Build();

// ... other middleware

// Add metrics endpoint
app.MapMetrics("/metrics");

// Add metrics to health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = PrometheusResponseWriter.WriteResponse
});
```

### Step 7: Custom Metrics in Services

#### TaskService with Metrics
```csharp
// TaskFlow.Application/Services/TaskService.cs
namespace TaskFlow.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ITaskRepository taskRepository, ILogger<TaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<Task> CreateTaskAsync(CreateTaskCommand command)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            var task = new Task(
                TaskTitle.From(command.Title),
                TaskDescription.From(command.Description),
                ProjectId.From(command.ProjectId),
                command.AssigneeId != null ? UserId.From(command.AssigneeId) : null
            );

            await _taskRepository.AddAsync(task);
            
            // Record metrics
            ApplicationMetrics.TaskCreatedCounter.Inc();
            ApplicationMetrics.ActiveTasksGauge.Inc();
            
            _logger.LogInformation("Task created: {TaskId} in {Duration}ms", 
                task.Id.Value, sw.ElapsedMilliseconds);
            
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create task in {Duration}ms", sw.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            ApplicationMetrics.DatabaseQueryDuration
                .WithLabels("create", "tasks")
                .Observe(sw.Elapsed.TotalSeconds);
        }
    }

    public async Task<Task?> GetTaskAsync(TaskId taskId)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            
            _logger.LogDebug("Task retrieved: {TaskId} in {Duration}ms", 
                taskId.Value, sw.ElapsedMilliseconds);
            
            return task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve task {TaskId} in {Duration}ms", 
                taskId.Value, sw.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            ApplicationMetrics.DatabaseQueryDuration
                .WithLabels("read", "tasks")
                .Observe(sw.Elapsed.TotalSeconds);
        }
    }
}
```

---

## 🔍 Observability Patterns

### Step 8: Distributed Tracing

#### Install OpenTelemetry Packages
```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Exporter.Jaeger
dotnet add package OpenTelemetry.Exporter.Zipkin
```

#### OpenTelemetry Configuration
```csharp
// Program.cs
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
        tracerProviderBuilder
            .AddSource("TaskFlow.API")
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName: "TaskFlow.API", serviceVersion: "1.0.0"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "localhost";
                options.AgentPort = 6831;
            })
            .AddZipkinExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            }))
    .WithMetrics(meterProviderBuilder =>
        meterProviderBuilder
            .AddMeter("TaskFlow.API")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation());

var app = builder.Build();
```

#### Activity Source
```csharp
// TaskFlow.API/Telemetry/ActivitySource.cs
namespace TaskFlow.API.Telemetry;

public static class ActivitySource
{
    public static readonly System.Diagnostics.ActivitySource Instance = 
        new("TaskFlow.API");
}
```

#### Enhanced Controller with Tracing
```csharp
// TaskFlow.API/Controllers/TasksController.cs
using TaskFlow.API.Telemetry;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTaskResponse>> CreateTask(CreateTaskCommand command)
    {
        using var activity = ActivitySource.Instance.StartActivity("CreateTask");
        activity?.SetTag("task.title", command.Title);
        activity?.SetTag("project.id", command.ProjectId);

        try
        {
            var result = await _mediator.Send(command);
            
            activity?.SetTag("task.id", result.TaskId);
            activity?.SetStatus(ActivityStatusCode.Ok);
            
            return CreatedAtAction(nameof(GetTask), new { id = result.TaskId }, result);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

---

## 📋 Configuration Files

### Step 9: appsettings.json Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/taskflow-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  },
  "HealthChecks": {
    "Database": {
      "ConnectionString": "DefaultConnection"
    },
    "ExternalServices": {
      "NotificationService": "https://notifications.example.com/health",
      "PaymentService": "https://payments.example.com/health"
    }
  },
  "Metrics": {
    "Enabled": true,
    "Endpoint": "/metrics"
  },
  "Tracing": {
    "Jaeger": {
      "Host": "localhost",
      "Port": 6831
    },
    "Zipkin": {
      "Endpoint": "http://localhost:9411/api/v2/spans"
    }
  }
}
```

---

## 🎯 Key Learning Points

### Logging Best Practices
1. **Structured Logging**: Use structured data, not string concatenation
2. **Log Levels**: Use appropriate levels (Debug, Info, Warning, Error, Fatal)
3. **Correlation IDs**: Track requests across services
4. **Context Enrichment**: Add relevant metadata to logs
5. **Performance**: Avoid expensive operations in logging

### Health Check Benefits
1. **Load Balancer Integration**: Automatic health-based routing
2. **Container Orchestration**: Kubernetes liveness/readiness probes
3. **Monitoring**: Proactive issue detection
4. **Dependencies**: Track external service health

### Metrics Best Practices
1. **Naming Conventions**: Use consistent metric names
2. **Labels**: Add relevant dimensions for filtering
3. **Cardinality**: Avoid high-cardinality labels
4. **Buckets**: Choose appropriate histogram buckets

### Observability Patterns
1. **Distributed Tracing**: Track requests across services
2. **Correlation**: Link logs, metrics, and traces
3. **Sampling**: Control trace volume in production
4. **Exporters**: Send data to monitoring systems

---

**Implementation Success**: Students can implement comprehensive logging, monitoring, and observability patterns in .NET APIs for production readiness. 