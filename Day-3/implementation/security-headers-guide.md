# Security Headers Implementation Guide - TaskFlow API

## 🎯 Overview
This guide implements comprehensive security headers in the TaskFlow API to protect against common web vulnerabilities including XSS, clickjacking, MIME sniffing, and other attacks.

## 🛡️ Security Headers Overview

| Header | Purpose | Protection Against |
|--------|---------|-------------------|
| `Strict-Transport-Security` | Forces HTTPS | Protocol downgrade attacks |
| `X-Frame-Options` | Prevents clickjacking | UI redressing attacks |
| `X-Content-Type-Options` | Prevents MIME sniffing | MIME confusion attacks |
| `X-XSS-Protection` | XSS protection | Cross-site scripting |
| `Content-Security-Policy` | Resource loading control | XSS, data injection |
| `Referrer-Policy` | Controls referrer info | Information leakage |
| `Permissions-Policy` | Feature permissions | Unwanted feature access |

## 🛠️ Implementation

### Step 1: Basic Security Headers Middleware

#### 1.1 Create Security Headers Middleware

```csharp
// src/TaskFlow.API/Middleware/SecurityHeadersMiddleware.cs
using Microsoft.AspNetCore.Http;

namespace TaskFlow.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()";
        
        // Add HSTS header for HTTPS requests
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        await _next(context);
    }
}
```

#### 1.2 Register Middleware in Program.cs

```csharp
// Add before UseRouting()
app.UseMiddleware<SecurityHeadersMiddleware>();
```

### Step 2: Content Security Policy (CSP)

#### 2.1 Create CSP Configuration

```csharp
// src/TaskFlow.API/Services/CspConfigurationService.cs
namespace TaskFlow.API.Services;

public class CspConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CspConfigurationService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public string GetCspHeader()
    {
        var cspBuilder = new CspBuilder();

        // Default source
        cspBuilder.DefaultSrc("'self'");

        // Script sources
        cspBuilder.ScriptSrc("'self'", "'unsafe-inline'", "'unsafe-eval'");

        // Style sources
        cspBuilder.StyleSrc("'self'", "'unsafe-inline'", "https://fonts.googleapis.com");

        // Font sources
        cspBuilder.FontSrc("'self'", "https://fonts.gstatic.com");

        // Image sources
        cspBuilder.ImgSrc("'self'", "data:", "https:");

        // Connect sources (for API calls)
        cspBuilder.ConnectSrc("'self'", "https://api.taskflow.com");

        // Frame sources
        cspBuilder.FrameSrc("'none'");

        // Object sources
        cspBuilder.ObjectSrc("'none'");

        // Media sources
        cspBuilder.MediaSrc("'self'");

        // Worker sources
        cspBuilder.WorkerSrc("'self'");

        // Manifest sources
        cspBuilder.ManifestSrc("'self'");

        // Form action
        cspBuilder.FormAction("'self'");

        // Base URI
        cspBuilder.BaseUri("'self'");

        // Frame ancestors
        cspBuilder.FrameAncestors("'none'");

        // Upgrade insecure requests
        cspBuilder.UpgradeInsecureRequests();

        return cspBuilder.Build();
    }
}

public class CspBuilder
{
    private readonly Dictionary<string, List<string>> _directives = new();

    public CspBuilder DefaultSrc(params string[] sources)
    {
        _directives["default-src"] = sources.ToList();
        return this;
    }

    public CspBuilder ScriptSrc(params string[] sources)
    {
        _directives["script-src"] = sources.ToList();
        return this;
    }

    public CspBuilder StyleSrc(params string[] sources)
    {
        _directives["style-src"] = sources.ToList();
        return this;
    }

    public CspBuilder FontSrc(params string[] sources)
    {
        _directives["font-src"] = sources.ToList();
        return this;
    }

    public CspBuilder ImgSrc(params string[] sources)
    {
        _directives["img-src"] = sources.ToList();
        return this;
    }

    public CspBuilder ConnectSrc(params string[] sources)
    {
        _directives["connect-src"] = sources.ToList();
        return this;
    }

    public CspBuilder FrameSrc(params string[] sources)
    {
        _directives["frame-src"] = sources.ToList();
        return this;
    }

    public CspBuilder ObjectSrc(params string[] sources)
    {
        _directives["object-src"] = sources.ToList();
        return this;
    }

    public CspBuilder MediaSrc(params string[] sources)
    {
        _directives["media-src"] = sources.ToList();
        return this;
    }

    public CspBuilder WorkerSrc(params string[] sources)
    {
        _directives["worker-src"] = sources.ToList();
        return this;
    }

    public CspBuilder ManifestSrc(params string[] sources)
    {
        _directives["manifest-src"] = sources.ToList();
        return this;
    }

    public CspBuilder FormAction(params string[] sources)
    {
        _directives["form-action"] = sources.ToList();
        return this;
    }

    public CspBuilder BaseUri(params string[] sources)
    {
        _directives["base-uri"] = sources.ToList();
        return this;
    }

    public CspBuilder FrameAncestors(params string[] sources)
    {
        _directives["frame-ancestors"] = sources.ToList();
        return this;
    }

    public CspBuilder UpgradeInsecureRequests()
    {
        _directives["upgrade-insecure-requests"] = new List<string>();
        return this;
    }

    public string Build()
    {
        return string.Join("; ", _directives.Select(d => $"{d.Key} {string.Join(" ", d.Value)}"));
    }
}
```

#### 2.2 Update Security Headers Middleware

```csharp
// src/TaskFlow.API/Middleware/SecurityHeadersMiddleware.cs
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly CspConfigurationService _cspService;

    public SecurityHeadersMiddleware(
        RequestDelegate next, 
        ILogger<SecurityHeadersMiddleware> logger,
        CspConfigurationService cspService)
    {
        _next = next;
        _logger = logger;
        _cspService = cspService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add basic security headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = GetPermissionsPolicy();
        
        // Add Content Security Policy
        context.Response.Headers["Content-Security-Policy"] = _cspService.GetCspHeader();
        
        // Add HSTS header for HTTPS requests
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Add additional security headers
        context.Response.Headers["X-Download-Options"] = "noopen";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        context.Response.Headers["X-DNS-Prefetch-Control"] = "off";

        await _next(context);
    }

    private string GetPermissionsPolicy()
    {
        return string.Join(", ", new[]
        {
            "geolocation=()",
            "microphone=()",
            "camera=()",
            "payment=()",
            "usb=()",
            "magnetometer=()",
            "gyroscope=()",
            "accelerometer=()",
            "ambient-light-sensor=()",
            "autoplay=()",
            "encrypted-media=()",
            "fullscreen=()",
            "picture-in-picture=()",
            "sync-xhr=()"
        });
    }
}
```

### Step 3: CORS Configuration

#### 3.1 Secure CORS Configuration

```csharp
// In Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureCors", policy =>
    {
        policy
            .WithOrigins("https://app.taskflow.com", "https://admin.taskflow.com")
            .AllowCredentials()
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "X-Requested-With")
            .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");
    });

    options.AddPolicy("PublicApi", policy =>
    {
        policy
            .WithOrigins("*")
            .WithMethods("GET")
            .WithHeaders("Content-Type");
    });
});

// In the pipeline
app.UseCors("SecureCors");
```

### Step 4: Environment-Specific Headers

#### 4.1 Development vs Production Headers

```csharp
// src/TaskFlow.API/Services/SecurityHeadersService.cs
public class SecurityHeadersService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SecurityHeadersService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public Dictionary<string, string> GetSecurityHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["X-Content-Type-Options"] = "nosniff",
            ["X-Frame-Options"] = "DENY",
            ["X-XSS-Protection"] = "1; mode=block",
            ["Referrer-Policy"] = "strict-origin-when-cross-origin",
            ["Permissions-Policy"] = GetPermissionsPolicy(),
            ["X-Download-Options"] = "noopen",
            ["X-Permitted-Cross-Domain-Policies"] = "none",
            ["X-DNS-Prefetch-Control"] = "off"
        };

        // Add HSTS only in production
        if (_environment.IsProduction())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Add CSP
        headers["Content-Security-Policy"] = GetCspPolicy();

        return headers;
    }

    private string GetCspPolicy()
    {
        if (_environment.IsDevelopment())
        {
            // More permissive for development
            return "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';";
        }

        // Strict for production
        return "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: https:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
    }

    private string GetPermissionsPolicy()
    {
        return string.Join(", ", new[]
        {
            "geolocation=()",
            "microphone=()",
            "camera=()",
            "payment=()",
            "usb=()",
            "magnetometer=()",
            "gyroscope=()",
            "accelerometer=()",
            "ambient-light-sensor=()",
            "autoplay=()",
            "encrypted-media=()",
            "fullscreen=()",
            "picture-in-picture=()",
            "sync-xhr=()"
        });
    }
}
```

### Step 5: Security Headers Validation

#### 5.1 Create Security Headers Validator

```csharp
// src/TaskFlow.API/Services/SecurityHeadersValidator.cs
public class SecurityHeadersValidator
{
    private readonly ILogger<SecurityHeadersValidator> _logger;

    public SecurityHeadersValidator(ILogger<SecurityHeadersValidator> logger)
    {
        _logger = logger;
    }

    public bool ValidateSecurityHeaders(IHeaderDictionary headers)
    {
        var requiredHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Referrer-Policy",
            "Permissions-Policy",
            "Content-Security-Policy"
        };

        var missingHeaders = requiredHeaders.Where(h => !headers.ContainsKey(h)).ToList();

        if (missingHeaders.Any())
        {
            _logger.LogWarning("Missing security headers: {Headers}", string.Join(", ", missingHeaders));
            return false;
        }

        // Validate specific header values
        if (headers["X-Content-Type-Options"] != "nosniff")
        {
            _logger.LogWarning("Invalid X-Content-Type-Options header value");
            return false;
        }

        if (headers["X-Frame-Options"] != "DENY" && headers["X-Frame-Options"] != "SAMEORIGIN")
        {
            _logger.LogWarning("Invalid X-Frame-Options header value");
            return false;
        }

        return true;
    }
}
```

### Step 6: Security Headers Testing

#### 6.1 Create Security Headers Test

```csharp
// src/TaskFlow.API.Tests/SecurityHeadersTests.cs
[TestClass]
public class SecurityHeadersTests
{
    private TestServer _server;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<TestStartup>();

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [TestMethod]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.IsTrue(response.Headers.Contains("X-Content-Type-Options"));
        Assert.IsTrue(response.Headers.Contains("X-Frame-Options"));
        Assert.IsTrue(response.Headers.Contains("X-XSS-Protection"));
        Assert.IsTrue(response.Headers.Contains("Referrer-Policy"));
        Assert.IsTrue(response.Headers.Contains("Permissions-Policy"));
        Assert.IsTrue(response.Headers.Contains("Content-Security-Policy"));
    }

    [TestMethod]
    public async Task SecurityHeaders_ShouldHaveCorrectValues()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.AreEqual("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.AreEqual("DENY", response.Headers.GetValues("X-Frame-Options").First());
        Assert.AreEqual("1; mode=block", response.Headers.GetValues("X-XSS-Protection").First());
        Assert.AreEqual("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").First());
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _server?.Dispose();
    }
}
```

### Step 7: Security Headers Monitoring

#### 7.1 Create Security Headers Monitor

```csharp
// src/TaskFlow.API/Services/SecurityHeadersMonitor.cs
public class SecurityHeadersMonitor
{
    private readonly ILogger<SecurityHeadersMonitor> _logger;
    private readonly IMetricsService _metricsService;

    public SecurityHeadersMonitor(ILogger<SecurityHeadersMonitor> logger, IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public void MonitorSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;
        var endpoint = context.Request.Path.Value;

        // Track security headers presence
        var securityHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Referrer-Policy",
            "Permissions-Policy",
            "Content-Security-Policy"
        };

        foreach (var header in securityHeaders)
        {
            if (headers.ContainsKey(header))
            {
                _metricsService.IncrementCounter("security_headers_present", new Dictionary<string, string>
                {
                    ["header"] = header,
                    ["endpoint"] = endpoint
                });
            }
            else
            {
                _logger.LogWarning("Missing security header: {Header} for endpoint: {Endpoint}", header, endpoint);
                _metricsService.IncrementCounter("security_headers_missing", new Dictionary<string, string>
                {
                    ["header"] = header,
                    ["endpoint"] = endpoint
                });
            }
        }
    }
}
```

## 🧪 Testing Security Headers

### 7.1 Manual Testing with curl

```bash
# Test security headers
curl -I https://localhost:7001/api/health

# Expected headers:
# X-Content-Type-Options: nosniff
# X-Frame-Options: DENY
# X-XSS-Protection: 1; mode=block
# Referrer-Policy: strict-origin-when-cross-origin
# Content-Security-Policy: default-src 'self'; ...
```

### 7.2 Automated Testing with Postman

```javascript
// Postman test script
pm.test("Security headers are present", function () {
    pm.response.to.have.header("X-Content-Type-Options");
    pm.response.to.have.header("X-Frame-Options");
    pm.response.to.have.header("X-XSS-Protection");
    pm.response.to.have.header("Referrer-Policy");
    pm.response.to.have.header("Content-Security-Policy");
});

pm.test("Security headers have correct values", function () {
    pm.expect(pm.response.headers.get("X-Content-Type-Options")).to.eql("nosniff");
    pm.expect(pm.response.headers.get("X-Frame-Options")).to.eql("DENY");
    pm.expect(pm.response.headers.get("X-XSS-Protection")).to.eql("1; mode=block");
});
```

## 📊 Security Headers Analysis

### 7.1 Security Headers Checker

```csharp
// src/TaskFlow.API/Controllers/SecurityController.cs
[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly SecurityHeadersValidator _validator;
    private readonly SecurityHeadersMonitor _monitor;

    public SecurityController(SecurityHeadersValidator validator, SecurityHeadersMonitor monitor)
    {
        _validator = validator;
        _monitor = monitor;
    }

    [HttpGet("headers")]
    public IActionResult GetSecurityHeaders()
    {
        var headers = Response.Headers;
        var validation = _validator.ValidateSecurityHeaders(headers);
        
        _monitor.MonitorSecurityHeaders(HttpContext);

        return Ok(new
        {
            headers = headers.Where(h => h.Key.StartsWith("X-") || h.Key.StartsWith("Content-") || h.Key.StartsWith("Referrer-") || h.Key.StartsWith("Permissions-") || h.Key.StartsWith("Strict-"))
                           .ToDictionary(h => h.Key, h => h.Value.ToString()),
            isValid = validation
        });
    }
}
```

## 🔍 Best Practices

### 1. Header Configuration
- **X-Content-Type-Options**: Always set to "nosniff"
- **X-Frame-Options**: Use "DENY" for maximum security, "SAMEORIGIN" if frames needed
- **X-XSS-Protection**: Use "1; mode=block" for older browsers
- **Referrer-Policy**: Use "strict-origin-when-cross-origin" for privacy
- **Content-Security-Policy**: Configure based on your application needs

### 2. Environment Considerations
- **Development**: More permissive CSP for debugging
- **Production**: Strict CSP with minimal allowed sources
- **Staging**: Mirror production settings

### 3. Monitoring and Alerting
- Monitor missing security headers
- Alert on security header changes
- Track security header effectiveness
- Log security header violations

### 4. Testing Strategy
- Automated tests for header presence
- Manual testing for header values
- Integration testing with frontend
- Security scanning tools validation

## 📋 Checklist

- [ ] Basic security headers implemented
- [ ] Content Security Policy configured
- [ ] CORS properly configured
- [ ] Environment-specific headers set
- [ ] Security headers validation added
- [ ] Automated testing implemented
- [ ] Monitoring and logging configured
- [ ] Documentation updated
- [ ] Security scan completed

## 🚀 Production Considerations

### 1. HSTS Preloading
Submit your domain to the HSTS preload list for maximum security.

### 2. CSP Reporting
Configure CSP violation reporting:

```csharp
context.Response.Headers["Content-Security-Policy-Report-Only"] = 
    "default-src 'self'; report-uri /api/security/csp-report;";
```

### 3. Security Headers Monitoring
Implement real-time monitoring of security headers:

```csharp
public void LogSecurityHeaderViolation(string header, string expected, string actual)
{
    _logger.LogWarning("Security header violation - Header: {Header}, Expected: {Expected}, Actual: {Actual}", 
        header, expected, actual);
    
    _metricsService.IncrementCounter("security_header_violation", new Dictionary<string, string>
    {
        ["header"] = header,
        ["expected"] = expected,
        ["actual"] = actual
    });
}
``` 