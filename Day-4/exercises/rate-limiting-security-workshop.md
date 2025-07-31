# Rate Limiting & Security Workshop

## 🎯 Workshop Objectives

**Implement production-ready rate limiting and security measures for APIs.**

### 📋 What You'll Learn
- Rate limiting algorithms and strategies
- Security headers and best practices
- Input validation and sanitization
- Monitoring and alerting setup
- Production security implementation

## 🚀 Workshop Structure

### Phase 1: Rate Limiting Implementation (60 minutes)

#### Step 1: Rate Limiting Algorithms
```csharp
// Token Bucket Algorithm
public class TokenBucketRateLimiter
{
    private readonly int _capacity;
    private readonly double _refillRate;
    private double _tokens;
    private DateTime _lastRefill;
    
    public TokenBucketRateLimiter(int capacity, double refillRate)
    {
        _capacity = capacity;
        _refillRate = refillRate;
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }
    
    public bool TryConsume(int tokens = 1)
    {
        RefillTokens();
        
        if (_tokens >= tokens)
        {
            _tokens -= tokens;
            return true;
        }
        
        return false;
    }
    
    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastRefill).TotalSeconds;
        var tokensToAdd = timePassed * _refillRate;
        
        _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
        _lastRefill = now;
    }
}

// Sliding Window Algorithm
public class SlidingWindowRateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private readonly Queue<DateTime> _requests;
    
    public SlidingWindowRateLimiter(int maxRequests, TimeSpan windowSize)
    {
        _maxRequests = maxRequests;
        _windowSize = windowSize;
        _requests = new Queue<DateTime>();
    }
    
    public bool TryConsume()
    {
        var now = DateTime.UtcNow;
        
        // Remove expired requests
        while (_requests.Count > 0 && now - _requests.Peek() > _windowSize)
        {
            _requests.Dequeue();
        }
        
        if (_requests.Count < _maxRequests)
        {
            _requests.Enqueue(now);
            return true;
        }
        
        return false;
    }
}
```

#### Step 2: Rate Limiting Middleware
```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    public RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path;
        
        var key = $"rate_limit:{clientId}:{endpoint}";
        var limit = GetRateLimit(clientId, endpoint);
        
        var currentCount = await GetCurrentCount(key);
        
        if (currentCount >= limit.MaxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}", clientId, endpoint);
            
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After", limit.Window.TotalSeconds.ToString());
            context.Response.Headers.Add("X-RateLimit-Limit", limit.MaxRequests.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", DateTime.UtcNow.Add(limit.Window).ToString("R"));
            
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        // Increment counter
        await IncrementCounter(key, limit.Window);
        
        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", limit.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (limit.MaxRequests - currentCount - 1).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", DateTime.UtcNow.Add(limit.Window).ToString("R"));
        
        await _next(context);
    }
    
    private string GetClientId(HttpContext context)
    {
        // Use API key, IP address, or user ID
        return context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
    
    private RateLimit GetRateLimit(string clientId, string endpoint)
    {
        // Different limits for different clients/endpoints
        if (clientId.StartsWith("premium_"))
        {
            return new RateLimit(1000, TimeSpan.FromMinutes(1));
        }
        
        return new RateLimit(100, TimeSpan.FromMinutes(1));
    }
    
    private async Task<int> GetCurrentCount(string key)
    {
        var value = await _cache.GetStringAsync(key);
        return int.TryParse(value, out var count) ? count : 0;
    }
    
    private async Task IncrementCounter(string key, TimeSpan window)
    {
        var current = await GetCurrentCount(key);
        await _cache.SetStringAsync(key, (current + 1).ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = window
        });
    }
}

public class RateLimit
{
    public int MaxRequests { get; }
    public TimeSpan Window { get; }
    
    public RateLimit(int maxRequests, TimeSpan window)
    {
        MaxRequests = maxRequests;
        Window = window;
    }
}
```

#### Step 3: Redis-Based Rate Limiting
```csharp
public class RedisRateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateLimiter> _logger;
    
    public RedisRateLimiter(IConnectionMultiplexer redis, ILogger<RedisRateLimiter> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window)
    {
        var db = _redis.GetDatabase();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var windowStart = now - (long)window.TotalSeconds;
        
        // Remove old entries and add new one
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);
        await db.SortedSetAddAsync(key, now, now);
        await db.KeyExpireAsync(key, window);
        
        var count = await db.SortedSetLengthAsync(key);
        
        return count <= maxRequests;
    }
}
```

### Phase 2: Security Implementation (60 minutes)

#### Step 1: Security Headers
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Security Headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=()");
        
        await _next(context);
    }
}

// Program.cs configuration
app.UseMiddleware<SecurityHeadersMiddleware>();
```

#### Step 2: Input Validation
```csharp
public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
            
        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter code");
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be valid");
            
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be valid");
            
        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");
    }
}

[ApiController]
public class PaymentsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        var validator = new PaymentRequestValidator();
        var validationResult = await validator.ValidateAsync(request);
        
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }
        
        // Process payment
        return Ok(new { success = true });
    }
}
```

#### Step 3: SQL Injection Prevention
```csharp
public class SecurePaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    
    public SecurePaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Payment> GetPaymentByIdAsync(int id)
    {
        // Use parameterized queries
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
    {
        // Use parameterized queries
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
    
    // Avoid raw SQL when possible
    // If you must use raw SQL, use parameters
    public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Payments
            .FromSqlRaw("SELECT * FROM Payments WHERE CreatedAt BETWEEN {0} AND {1}", start, end)
            .ToListAsync();
    }
}
```

### Phase 3: Monitoring & Alerting (30 minutes)

#### Step 1: Request Monitoring
```csharp
public class RequestMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestMonitoringMiddleware> _logger;
    private readonly IMetrics _metrics;
    
    public RequestMonitoringMiddleware(RequestDelegate next, ILogger<RequestMonitoringMiddleware> logger, IMetrics metrics)
    {
        _next = next;
        _logger = logger;
        _metrics = metrics;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var path = context.Request.Path;
        var method = context.Request.Method;
        
        try
        {
            await _next(context);
            
            // Record metrics
            _metrics.Increment($"requests.{method}.{path}.{context.Response.StatusCode}");
            _metrics.Timing($"response_time.{method}.{path}", stopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                method, path, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _metrics.Increment($"errors.{method}.{path}");
            _logger.LogError(ex, "Request {Method} {Path} failed after {ElapsedMs}ms",
                method, path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

#### Step 2: Security Monitoring
```csharp
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly IMetrics _metrics;
    
    public SecurityMonitoringService(ILogger<SecurityMonitoringService> logger, IMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }
    
    public void LogSuspiciousActivity(string activity, string details, string clientId)
    {
        _metrics.Increment("security.suspicious_activity");
        _logger.LogWarning("Suspicious activity detected: {Activity} - {Details} from {ClientId}",
            activity, details, clientId);
    }
    
    public void LogRateLimitExceeded(string clientId, string endpoint)
    {
        _metrics.Increment("security.rate_limit_exceeded");
        _logger.LogWarning("Rate limit exceeded for {ClientId} on {Endpoint}", clientId, endpoint);
    }
    
    public void LogFailedAuthentication(string clientId, string reason)
    {
        _metrics.Increment("security.auth_failure");
        _logger.LogWarning("Authentication failed for {ClientId}: {Reason}", clientId, reason);
    }
}
```

## 🛠️ Hands-On Exercise

### Exercise 1: Implement Rate Limiting for Squad.API
```csharp
// Add rate limiting to Squad.API endpoints
// Implement different limits for different user tiers
// Add monitoring and alerting
```

**Your Task**:
1. Implement token bucket rate limiting
2. Add rate limiting middleware
3. Configure different limits for different endpoints
4. Add rate limit headers to responses
5. Implement monitoring and alerting

### Exercise 2: Security Hardening
```csharp
// Enhance Squad.API security
// Add comprehensive security headers
// Implement input validation
// Add security monitoring
```

**Your Task**:
1. Add security headers middleware
2. Implement comprehensive input validation
3. Add SQL injection prevention
4. Implement security monitoring
5. Add security alerting

### Exercise 3: Production Monitoring
```csharp
// Set up production monitoring for Squad.API
// Implement metrics collection
// Add alerting rules
// Create dashboards
```

**Your Task**:
1. Implement request monitoring
2. Add performance metrics
3. Set up error tracking
4. Create alerting rules
5. Build monitoring dashboards

## 📊 Assessment Criteria

### Rate Limiting Implementation (40%)
- ✅ Multiple rate limiting algorithms implemented
- ✅ Different limits for different user tiers
- ✅ Proper rate limit headers
- ✅ Redis-based distributed rate limiting

### Security Implementation (40%)
- ✅ Comprehensive security headers
- ✅ Input validation and sanitization
- ✅ SQL injection prevention
- ✅ Security monitoring and alerting

### Monitoring & Alerting (20%)
- ✅ Request metrics collection
- ✅ Performance monitoring
- ✅ Error tracking
- ✅ Security incident alerting

## 🎯 Success Metrics

### By End of Workshop, You Should:
1. ✅ Implement production-ready rate limiting
2. ✅ Add comprehensive security measures
3. ✅ Set up monitoring and alerting
4. ✅ Handle security incidents properly
5. ✅ Monitor API performance and usage

## 📚 Additional Resources

### Tools & Libraries
- **Redis** - Distributed rate limiting
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Prometheus** - Metrics collection
- **Grafana** - Monitoring dashboards

### Documentation
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Rate Limiting Best Practices](https://cloud.google.com/architecture/rate-limiting-strategies-techniques)
- [OWASP Security Guidelines](https://owasp.org/www-project-api-security/)

### Best Practices
- Always validate and sanitize input
- Use parameterized queries
- Implement defense in depth
- Monitor and alert on security events
- Regular security audits
- Keep dependencies updated

---

**Remember**: Security is not a one-time implementation. It's an ongoing process of monitoring, updating, and improving your security posture! 