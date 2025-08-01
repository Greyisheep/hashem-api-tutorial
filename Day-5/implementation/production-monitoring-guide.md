# Production Monitoring Guide

## 🎯 Overview

This guide covers comprehensive monitoring and observability for production applications. You'll learn structured logging, metrics collection, alerting, and performance monitoring.

## 📋 Prerequisites

- .NET 8 application ready for production
- Docker containers deployed
- Azure account (for Application Insights)
- Prometheus and Grafana setup

---

## 📝 Part 1: Structured Logging

### Step 1: Serilog Configuration

```csharp
// Program.cs
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(builder.Configuration["ApplicationInsights:ConnectionString"], TelemetryConverter.Traces)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Ensure logs are flushed on shutdown
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
```

### Step 2: Structured Logging Implementation

```csharp
// TasksController.cs
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ILogger<TasksController> _logger;
    private readonly ITaskService _taskService;

    public TasksController(ILogger<TasksController> logger, ITaskService taskService)
    {
        _logger = logger;
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var correlationId = HttpContext.TraceIdentifier;
        
        _logger.LogInformation("Getting tasks for user {UserId} with correlation ID {CorrelationId}", 
            User.Identity?.Name, correlationId);

        try
        {
            var tasks = await _taskService.GetTasksAsync();
            
            _logger.LogInformation("Successfully retrieved {TaskCount} tasks for user {UserId}", 
                tasks.Count, User.Identity?.Name);
                
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user {UserId} with correlation ID {CorrelationId}", 
                User.Identity?.Name, correlationId);
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var correlationId = HttpContext.TraceIdentifier;
        
        _logger.LogInformation("Creating task '{TaskTitle}' for user {UserId} with correlation ID {CorrelationId}", 
            request.Title, User.Identity?.Name, correlationId);

        try
        {
            var task = await _taskService.CreateTaskAsync(request);
            
            _logger.LogInformation("Successfully created task {TaskId} '{TaskTitle}' for user {UserId}", 
                task.Id, task.Title, User.Identity?.Name);
                
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for task creation: {ValidationErrors}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for user {UserId} with correlation ID {CorrelationId}", 
                User.Identity?.Name, correlationId);
            throw;
        }
    }
}
```

### Step 3: Log Enrichment

```csharp
// LoggingMiddleware.cs
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = context.TraceIdentifier;

        // Add correlation ID to log context
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
        using (LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString()))
        {
            _logger.LogInformation("Request started: {Method} {Path} from {ClientIP}", 
                context.Request.Method, context.Request.Path, context.Connection.RemoteIpAddress);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {Duration}ms", 
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, duration.TotalMilliseconds);
            }
        }
    }
}
```

---

## 📊 Part 2: Metrics Collection

### Step 1: Prometheus Metrics

```csharp
// Metrics configuration
public class MetricsConfig
{
    public static void ConfigureMetrics(IServiceCollection services)
    {
        services.AddMetrics();
        
        // Configure Prometheus metrics
        services.AddPrometheusMetrics();
    }
}

// Custom metrics
public class ApplicationMetrics
{
    private readonly Counter _requestCounter;
    private readonly Histogram _requestDuration;
    private readonly Gauge _activeConnections;
    private readonly Counter _errorCounter;

    public ApplicationMetrics(IMetricsFactory metricsFactory)
    {
        _requestCounter = metricsFactory.CreateCounter("http_requests_total", "Total HTTP requests");
        _requestDuration = metricsFactory.CreateHistogram("http_request_duration_seconds", "HTTP request duration");
        _activeConnections = metricsFactory.CreateGauge("active_connections", "Active database connections");
        _errorCounter = metricsFactory.CreateCounter("errors_total", "Total errors");
    }

    public void RecordRequest(string method, string path, int statusCode)
    {
        _requestCounter.Add(1, new KeyValuePair<string, object>("method", method));
        _requestCounter.Add(1, new KeyValuePair<string, object>("path", path));
        _requestCounter.Add(1, new KeyValuePair<string, object>("status_code", statusCode.ToString()));
    }

    public IDisposable MeasureRequest()
    {
        return _requestDuration.NewTimer();
    }

    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }

    public void RecordError(string errorType, string errorMessage)
    {
        _errorCounter.Add(1, new KeyValuePair<string, object>("error_type", errorType));
        _errorCounter.Add(1, new KeyValuePair<string, object>("error_message", errorMessage));
    }
}
```

### Step 2: Metrics Middleware

```csharp
// MetricsMiddleware.cs
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApplicationMetrics _metrics;

    public MetricsMiddleware(RequestDelegate next, ApplicationMetrics metrics)
    {
        _next = next;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            
            _metrics.RecordRequest(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);

            using (_metrics.MeasureRequest())
            {
                // Timer will automatically record the duration
            }
        }
    }
}
```

---

## 🚨 Part 3: Alerting and Notifications

### Step 1: Health Check Alerts

```csharp
// HealthCheckController.cs
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IDbContext _dbContext;
    private readonly IRedisService _redisService;
    private readonly ApplicationMetrics _metrics;

    public HealthController(
        ILogger<HealthController> logger,
        IDbContext dbContext,
        IRedisService redisService,
        ApplicationMetrics metrics)
    {
        _logger = logger;
        _dbContext = dbContext;
        _redisService = redisService;
        _metrics = metrics;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthChecks = new List<HealthCheckResult>();
        var isHealthy = true;

        // Database health check
        try
        {
            await _dbContext.Database.CanConnectAsync();
            healthChecks.Add(new HealthCheckResult("Database", "Healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            healthChecks.Add(new HealthCheckResult("Database", "Unhealthy", ex.Message));
            _metrics.RecordError("Database", "Connection failed");
            isHealthy = false;
        }

        // Redis health check
        try
        {
            await _redisService.PingAsync();
            healthChecks.Add(new HealthCheckResult("Redis", "Healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            healthChecks.Add(new HealthCheckResult("Redis", "Unhealthy", ex.Message));
            _metrics.RecordError("Redis", "Connection failed");
            isHealthy = false;
        }

        // Memory health check
        var memoryInfo = GC.GetGCMemoryInfo();
        var memoryUsage = memoryInfo.HeapSizeBytes / (1024 * 1024); // MB
        
        if (memoryUsage > 1024) // Alert if memory usage > 1GB
        {
            _logger.LogWarning("High memory usage detected: {MemoryUsage}MB", memoryUsage);
            _metrics.RecordError("Memory", $"High usage: {memoryUsage}MB");
        }

        healthChecks.Add(new HealthCheckResult("Memory", "Healthy", $"Usage: {memoryUsage}MB"));

        var statusCode = isHealthy ? 200 : 503;

        return StatusCode(statusCode, new
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = healthChecks
        });
    }
}
```

### Step 2: Alert Rules (Prometheus)

```yaml
# prometheus-rules.yml
groups:
  - name: application_alerts
    rules:
      - alert: HighErrorRate
        expr: rate(errors_total[5m]) > 0.1
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value }} errors per second"

      - alert: HighResponseTime
        expr: histogram_quantile(0.95, http_request_duration_seconds_bucket) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "95th percentile response time is {{ $value }} seconds"

      - alert: HighMemoryUsage
        expr: process_resident_memory_bytes / 1024 / 1024 > 1024
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High memory usage detected"
          description: "Memory usage is {{ $value }} MB"

      - alert: ServiceDown
        expr: up == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Service is down"
          description: "Service has been down for more than 1 minute"
```

---

## 📈 Part 4: Performance Monitoring

### Step 1: Application Performance Monitoring

```csharp
// Performance monitoring
public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly ApplicationMetrics _metrics;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger, ApplicationMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<T> MeasureAsync<T>(string operation, Func<Task<T>> operationFunc)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await operationFunc();
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("Operation {Operation} completed in {Duration}ms", 
                operation, duration.TotalMilliseconds);
                
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Operation {Operation} failed after {Duration}ms", 
                operation, duration.TotalMilliseconds);
            throw;
        }
    }
}

// Usage in service
public class TaskService : ITaskService
{
    private readonly PerformanceMonitor _performanceMonitor;

    public async Task<List<Task>> GetTasksAsync()
    {
        return await _performanceMonitor.MeasureAsync("GetTasks", async () =>
        {
            // Actual implementation
            return await _repository.GetAllAsync();
        });
    }
}
```

### Step 2: Database Performance Monitoring

```csharp
// Database performance monitoring
public class DatabasePerformanceMonitor
{
    private readonly ILogger<DatabasePerformanceMonitor> _logger;
    private readonly ApplicationMetrics _metrics;

    public DatabasePerformanceMonitor(ILogger<DatabasePerformanceMonitor> logger, ApplicationMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<T> MeasureQueryAsync<T>(string queryName, Func<Task<T>> queryFunc)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = await queryFunc();
            
            var duration = DateTime.UtcNow - startTime;
            
            if (duration.TotalMilliseconds > 1000) // Alert for slow queries
            {
                _logger.LogWarning("Slow query detected: {QueryName} took {Duration}ms", 
                    queryName, duration.TotalMilliseconds);
                _metrics.RecordError("Database", $"Slow query: {queryName}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Query {QueryName} failed after {Duration}ms", 
                queryName, duration.TotalMilliseconds);
            _metrics.RecordError("Database", $"Query failed: {queryName}");
            throw;
        }
    }
}
```

---

## 📊 Part 5: Grafana Dashboards

### Step 1: Application Dashboard

```json
{
  "dashboard": {
    "title": "TaskFlow API Dashboard",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{path}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, http_request_duration_seconds_bucket)",
            "legendFormat": "95th percentile"
          },
          {
            "expr": "histogram_quantile(0.50, http_request_duration_seconds_bucket)",
            "legendFormat": "50th percentile"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(errors_total[5m])",
            "legendFormat": "{{error_type}}"
          }
        ]
      },
      {
        "title": "Memory Usage",
        "type": "graph",
        "targets": [
          {
            "expr": "process_resident_memory_bytes / 1024 / 1024",
            "legendFormat": "Memory (MB)"
          }
        ]
      },
      {
        "title": "Active Connections",
        "type": "stat",
        "targets": [
          {
            "expr": "active_connections",
            "legendFormat": "Connections"
          }
        ]
      }
    ]
  }
}
```

### Step 2: Business Metrics Dashboard

```json
{
  "dashboard": {
    "title": "Business Metrics",
    "panels": [
      {
        "title": "Tasks Created",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(tasks_created_total[1h])",
            "legendFormat": "Tasks/hour"
          }
        ]
      },
      {
        "title": "Active Users",
        "type": "stat",
        "targets": [
          {
            "expr": "active_users",
            "legendFormat": "Users"
          }
        ]
      },
      {
        "title": "Task Completion Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(tasks_completed_total[1h]) / rate(tasks_created_total[1h]) * 100",
            "legendFormat": "Completion %"
          }
        ]
      }
    ]
  }
}
```

---

## 🔧 Part 6: Monitoring Configuration

### Step 1: Docker Compose with Monitoring

```yaml
# docker-compose.monitoring.yml
version: '3.8'

services:
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana

  alertmanager:
    image: prom/alertmanager:latest
    ports:
      - "9093:9093"
    volumes:
      - ./alertmanager.yml:/etc/alertmanager/alertmanager.yml
    command:
      - '--config.file=/etc/alertmanager/alertmanager.yml'
      - '--storage.path=/alertmanager'

volumes:
  prometheus_data:
  grafana_data:
```

### Step 2: Prometheus Configuration

```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "prometheus-rules.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  - job_name: 'taskflow-api'
    static_configs:
      - targets: ['taskflow-api:8080']
    metrics_path: '/metrics'
    scrape_interval: 5s

  - job_name: 'node-exporter'
    static_configs:
      - targets: ['node-exporter:9100']
```

---

## 🎯 Key Takeaways

### What You've Learned
1. **Structured Logging**: Comprehensive logging with Serilog
2. **Metrics Collection**: Prometheus metrics and custom counters
3. **Health Checks**: Application and dependency health monitoring
4. **Alerting**: Automated alerts for critical issues
5. **Performance Monitoring**: Request timing and database performance

### Best Practices
1. **Always use structured logging** with correlation IDs
2. **Monitor key business metrics** alongside technical metrics
3. **Set up automated alerts** for critical issues
4. **Track performance trends** over time
5. **Implement health checks** for all dependencies

### Next Steps
1. **Set up monitoring infrastructure** (Prometheus, Grafana)
2. **Implement structured logging** in your applications
3. **Create custom metrics** for business KPIs
4. **Set up alerting rules** for critical thresholds
5. **Create dashboards** for different stakeholders

This guide provides a comprehensive monitoring solution that you can adapt for your specific needs. The key is to monitor both technical and business metrics to ensure your application is performing well. 