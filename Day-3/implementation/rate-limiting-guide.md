# Rate Limiting Implementation Guide - TaskFlow API

## 🎯 Overview
This guide implements comprehensive rate limiting in the TaskFlow API to protect against brute force attacks, scraping, and DoS attacks.

## 🚦 Rate Limiting Strategies

### Fixed Window
- **Description**: Allow X requests per time window (e.g., 100 requests per minute)
- **Pros**: Simple to implement and understand
- **Cons**: Can allow bursts at window boundaries

### Sliding Window
- **Description**: Track requests in overlapping time windows
- **Pros**: More accurate rate limiting
- **Cons**: More complex implementation

### Token Bucket
- **Description**: Requests consume tokens, tokens refill over time
- **Pros**: Smooths out bursts, fair to users
- **Cons**: More complex to implement

## 🛠️ Implementation

### Step 1: Install Required Packages

```bash
cd taskflow-api-dotnet
dotnet add src/TaskFlow.API/TaskFlow.API.csproj package AspNetCoreRateLimit
```

### Step 2: Configure Rate Limiting

#### 2.1 Update Program.cs

```csharp
// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 1000,
            Period = "1h"
        }
    };
});

builder.Services.Configure<ClientRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 50,
            Period = "1m"
        }
    };
});

builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

#### 2.2 Add Rate Limiting Middleware

```csharp
// Add after UseRouting() and before UseAuthentication()
app.UseIpRateLimiting();
app.UseClientRateLimiting();
```

### Step 3: Custom Rate Limiting Implementation

#### 3.1 Create Rate Limiting Service

```csharp
// src/TaskFlow.API/Services/RateLimitingService.cs
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace TaskFlow.API.Services;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string key);
}

public class RateLimitingService : IRateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(IMemoryCache cache, ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window)
    {
        var cacheKey = $"rate_limit:{key}";
        var requests = await GetRequestsAsync(cacheKey);
        
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(window);
        
        // Remove expired requests
        requests.RemoveAll(r => r < windowStart);
        
        if (requests.Count >= maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
            return false;
        }
        
        requests.Add(now);
        _cache.Set(cacheKey, requests, window);
        
        return true;
    }

    public async Task<RateLimitInfo> GetRateLimitInfoAsync(string key)
    {
        var cacheKey = $"rate_limit:{key}";
        var requests = await GetRequestsAsync(cacheKey);
        
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(TimeSpan.FromMinutes(1));
        
        requests.RemoveAll(r => r < windowStart);
        
        return new RateLimitInfo
        {
            CurrentRequests = requests.Count,
            RemainingRequests = Math.Max(0, 100 - requests.Count),
            ResetTime = now.AddMinutes(1)
        };
    }

    private async Task<List<DateTime>> GetRequestsAsync(string cacheKey)
    {
        if (_cache.TryGetValue(cacheKey, out List<DateTime> requests))
        {
            return requests;
        }
        
        return new List<DateTime>();
    }
}

public class RateLimitInfo
{
    public int CurrentRequests { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
}
```

#### 3.2 Create Rate Limiting Middleware

```csharp
// src/TaskFlow.API/Middleware/RateLimitingMiddleware.cs
using TaskFlow.API.Services;

namespace TaskFlow.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitingService rateLimitingService,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetRateLimitKey(context);
        var isAllowed = await _rateLimitingService.IsAllowedAsync(key, 100, TimeSpan.FromMinutes(1));
        
        if (!isAllowed)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            
            var rateLimitInfo = await _rateLimitingService.GetRateLimitInfoAsync(key);
            var response = new
            {
                error = "Rate limit exceeded",
                retryAfter = (int)(rateLimitInfo.ResetTime - DateTime.UtcNow).TotalSeconds,
                limit = 100,
                remaining = rateLimitInfo.RemainingRequests
            };
            
            await context.Response.WriteAsJsonAsync(response);
            return;
        }
        
        // Add rate limit headers
        var info = await _rateLimitingService.GetRateLimitInfoAsync(key);
        context.Response.Headers["X-RateLimit-Limit"] = "100";
        context.Response.Headers["X-RateLimit-Remaining"] = info.RemainingRequests.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = info.ResetTime.ToString("R");
        
        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        // Use IP address for anonymous requests
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Use user ID for authenticated requests
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }
        
        return $"ip:{ip}";
    }
}
```

### Step 4: Per-Endpoint Rate Limiting

#### 4.1 Create Rate Limiting Attributes

```csharp
// src/TaskFlow.API/Attributes/RateLimitAttribute.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.API.Services;

namespace TaskFlow.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RateLimitAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _maxRequests;
    private readonly int _windowMinutes;
    private readonly string _keyPrefix;

    public RateLimitAttribute(int maxRequests, int windowMinutes, string keyPrefix = "")
    {
        _maxRequests = maxRequests;
        _windowMinutes = windowMinutes;
        _keyPrefix = keyPrefix;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var rateLimitingService = context.HttpContext.RequestServices.GetRequiredService<IRateLimitingService>();
        var key = GetRateLimitKey(context);
        
        var isAllowed = await rateLimitingService.IsAllowedAsync(key, _maxRequests, TimeSpan.FromMinutes(_windowMinutes));
        
        if (!isAllowed)
        {
            context.Result = new ObjectResult(new { error = "Rate limit exceeded" })
            {
                StatusCode = 429
            };
            return;
        }
        
        await next();
    }

    private string GetRateLimitKey(ActionExecutingContext context)
    {
        var baseKey = _keyPrefix;
        var userId = context.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!string.IsNullOrEmpty(userId))
        {
            return $"{baseKey}:user:{userId}";
        }
        
        return $"{baseKey}:ip:{ip}";
    }
}
```

#### 4.2 Apply to Controllers

```csharp
// src/TaskFlow.API/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[RateLimit(100, 1)] // 100 requests per minute
public class TasksController : ControllerBase
{
    [HttpGet]
    [RateLimit(50, 1, "tasks:list")] // 50 requests per minute for listing
    public async Task<IActionResult> GetTasks()
    {
        // Implementation
    }

    [HttpPost]
    [RateLimit(10, 1, "tasks:create")] // 10 requests per minute for creation
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
    {
        // Implementation
    }
}
```

### Step 5: Advanced Rate Limiting Strategies

#### 5.1 User-Based Rate Limiting

```csharp
// src/TaskFlow.API/Services/UserRateLimitingService.cs
public class UserRateLimitingService
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<UserRateLimitingService> _logger;

    public UserRateLimitingService(IRateLimitingService rateLimitingService, ILogger<UserRateLimitingService> logger)
    {
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task<bool> IsUserAllowedAsync(string userId, string endpoint, int maxRequests, TimeSpan window)
    {
        var key = $"user:{userId}:{endpoint}";
        return await _rateLimitingService.IsAllowedAsync(key, maxRequests, window);
    }

    public async Task<bool> IsUserAllowedForResourceAsync(string userId, string resourceType, string resourceId, int maxRequests, TimeSpan window)
    {
        var key = $"user:{userId}:{resourceType}:{resourceId}";
        return await _rateLimitingService.IsAllowedAsync(key, maxRequests, window);
    }
}
```

#### 5.2 API Key Rate Limiting

```csharp
// src/TaskFlow.API/Services/ApiKeyRateLimitingService.cs
public class ApiKeyRateLimitingService
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<ApiKeyRateLimitingService> _logger;

    public ApiKeyRateLimitingService(IRateLimitingService rateLimitingService, ILogger<ApiKeyRateLimitingService> logger)
    {
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task<bool> IsApiKeyAllowedAsync(string apiKey, string plan, int maxRequests, TimeSpan window)
    {
        var key = $"apikey:{apiKey}:{plan}";
        return await _rateLimitingService.IsAllowedAsync(key, maxRequests, window);
    }

    public async Task<RateLimitInfo> GetApiKeyRateLimitInfoAsync(string apiKey, string plan)
    {
        var key = $"apikey:{apiKey}:{plan}";
        return await _rateLimitingService.GetRateLimitInfoAsync(key);
    }
}
```

### Step 6: Rate Limiting Configuration

#### 6.1 Update appsettings.json

```json
{
  "RateLimiting": {
    "Default": {
      "MaxRequests": 100,
      "WindowMinutes": 1
    },
    "Endpoints": {
      "Tasks": {
        "List": {
          "MaxRequests": 50,
          "WindowMinutes": 1
        },
        "Create": {
          "MaxRequests": 10,
          "WindowMinutes": 1
        }
      },
      "Auth": {
        "Login": {
          "MaxRequests": 5,
          "WindowMinutes": 5
        }
      }
    },
    "Plans": {
      "Free": {
        "MaxRequests": 100,
        "WindowMinutes": 1
      },
      "Pro": {
        "MaxRequests": 1000,
        "WindowMinutes": 1
      },
      "Enterprise": {
        "MaxRequests": 10000,
        "WindowMinutes": 1
      }
    }
  }
}
```

#### 6.2 Configuration Service

```csharp
// src/TaskFlow.API/Services/RateLimitConfigurationService.cs
public class RateLimitConfigurationService
{
    private readonly IConfiguration _configuration;

    public RateLimitConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public RateLimitConfig GetEndpointConfig(string endpoint)
    {
        var config = _configuration.GetSection($"RateLimiting:Endpoints:{endpoint}");
        return new RateLimitConfig
        {
            MaxRequests = config.GetValue<int>("MaxRequests", 100),
            WindowMinutes = config.GetValue<int>("WindowMinutes", 1)
        };
    }

    public RateLimitConfig GetPlanConfig(string plan)
    {
        var config = _configuration.GetSection($"RateLimiting:Plans:{plan}");
        return new RateLimitConfig
        {
            MaxRequests = config.GetValue<int>("MaxRequests", 100),
            WindowMinutes = config.GetValue<int>("WindowMinutes", 1)
        };
    }
}

public class RateLimitConfig
{
    public int MaxRequests { get; set; }
    public int WindowMinutes { get; set; }
}
```

### Step 7: Monitoring and Logging

#### 7.1 Rate Limit Monitoring

```csharp
// src/TaskFlow.API/Services/RateLimitMonitoringService.cs
public class RateLimitMonitoringService
{
    private readonly ILogger<RateLimitMonitoringService> _logger;
    private readonly IMetricsService _metricsService;

    public RateLimitMonitoringService(ILogger<RateLimitMonitoringService> logger, IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public void LogRateLimitExceeded(string key, string endpoint, string userId = null)
    {
        _logger.LogWarning("Rate limit exceeded - Key: {Key}, Endpoint: {Endpoint}, User: {UserId}", 
            key, endpoint, userId ?? "anonymous");
        
        _metricsService.IncrementCounter("rate_limit_exceeded", new Dictionary<string, string>
        {
            ["endpoint"] = endpoint,
            ["user_type"] = userId != null ? "authenticated" : "anonymous"
        });
    }

    public void LogRateLimitInfo(string key, int currentRequests, int maxRequests)
    {
        _metricsService.RecordGauge("rate_limit_usage", currentRequests, new Dictionary<string, string>
        {
            ["key"] = key
        });
    }
}
```

## 🧪 Testing Rate Limiting

### 7.1 Test with Postman

```bash
# Test rate limiting
for i in {1..110}; do
  curl -X GET "https://localhost:7001/api/tasks" \
    -H "Authorization: Bearer YOUR_JWT_TOKEN" \
    -H "Content-Type: application/json"
done
```

### 7.2 Test with Load Testing Tool

```bash
# Using Apache Bench
ab -n 1000 -c 10 -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  https://localhost:7001/api/tasks
```

## 📊 Rate Limiting Headers

The API will return these headers:

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: Wed, 21 Oct 2023 07:28:00 GMT
Retry-After: 60
```

## 🔍 Best Practices

### 1. Choose Appropriate Limits
- **Public endpoints**: 100-1000 requests per minute
- **Authentication endpoints**: 5-10 requests per minute
- **Resource creation**: 10-50 requests per minute
- **Data export**: 1-5 requests per minute

### 2. Use Different Keys
- **IP-based**: For anonymous users
- **User-based**: For authenticated users
- **API key-based**: For third-party integrations
- **Endpoint-based**: For specific operations

### 3. Provide Clear Feedback
- Return proper HTTP status codes (429)
- Include retry-after headers
- Log rate limit violations
- Monitor rate limit usage

### 4. Consider Business Logic
- Different limits for different user tiers
- Adjust limits based on user behavior
- Implement progressive rate limiting
- Allow burst requests for legitimate users

## 📋 Checklist

- [ ] AspNetCoreRateLimit package installed
- [ ] Rate limiting middleware configured
- [ ] Custom rate limiting service implemented
- [ ] Per-endpoint rate limiting attributes created
- [ ] Rate limiting headers added
- [ ] Monitoring and logging implemented
- [ ] Configuration externalized
- [ ] Testing completed
- [ ] Documentation updated

## 🚀 Production Considerations

### 1. Distributed Rate Limiting
For multi-instance deployments, use Redis:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddSingleton<IRateLimitingService, RedisRateLimitingService>();
```

### 2. Rate Limit Bypass
Implement whitelist for trusted IPs:

```csharp
private bool IsWhitelisted(string ip)
{
    var whitelist = new[] { "10.0.0.0/8", "192.168.0.0/16" };
    return whitelist.Any(range => IsInRange(ip, range));
}
```

### 3. Rate Limit Analytics
Track rate limit usage for optimization:

```csharp
public void TrackRateLimitUsage(string key, int requests, int limit)
{
    _analyticsService.Track("rate_limit_usage", new
    {
        key,
        requests,
        limit,
        percentage = (double)requests / limit * 100
    });
}
``` 