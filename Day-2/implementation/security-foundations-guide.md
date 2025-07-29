# Security Foundations Implementation Guide - .NET

## 🎯 Implementation Objectives
- Implement JWT authentication in .NET APIs
- Set up role-based authorization
- Apply security best practices and input validation
- Create secure API endpoints with proper error handling

## ⏰ Implementation Timing: 45 minutes

---

## 🔐 Authentication Implementation

### Step 1: Install Security Packages

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.IdentityModel.Tokens
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next
```

### Step 2: JWT Configuration

#### appsettings.json Security Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-256-bits",
    "Issuer": "TaskFlow-API",
    "Audience": "TaskFlow-Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Security": {
    "PasswordPolicy": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "RateLimiting": {
      "MaxRequestsPerMinute": 100,
      "MaxRequestsPerHour": 1000
    }
  }
}
```

#### JWT Service Implementation
```csharp
// TaskFlow.Infrastructure/Services/JwtService.cs
namespace TaskFlow.Infrastructure.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    bool ValidateRefreshToken(string refreshToken);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value),
            new(ClaimTypes.Email, user.Email.Value),
            new(ClaimTypes.Name, user.Name.Value),
            new(ClaimTypes.Role, user.Role.Value),
            new("permissions", string.Join(",", user.Permissions.Select(p => p.Value)))
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = secretKey,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        // In production, validate against stored refresh tokens
        return !string.IsNullOrEmpty(refreshToken) && refreshToken.Length >= 64;
    }
}
```

### Step 3: Authentication Controller

#### AuthController
```csharp
// TaskFlow.API/Controllers/AuthController.cs
namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IMediator mediator, 
        IJwtService jwtService, 
        ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication token and user information</returns>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates user credentials and returns JWT token",
        OperationId = "Login",
        Tags = new[] { "Authentication" }
    )]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Unauthorized(new { error = "Invalid credentials" });
            }

            var token = _jwtService.GenerateToken(result.User);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Store refresh token (in production, save to database)
            await _mediator.Send(new StoreRefreshTokenCommand
            {
                UserId = result.User.Id.Value,
                RefreshToken = refreshToken
            });

            _logger.LogInformation("Successful login for user: {UserId}", result.User.Id.Value);

            return Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                User = new UserDto
                {
                    Id = result.User.Id.Value,
                    Email = result.User.Email.Value,
                    Name = result.User.Name.Value,
                    Role = result.User.Role.Value
                }
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error during login");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes an expired JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [SwaggerOperation(
        Summary = "Refresh token",
        Description = "Refreshes an expired JWT token using a valid refresh token",
        OperationId = "RefreshToken",
        Tags = new[] { "Authentication" }
    )]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };

            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Invalid refresh token attempt");
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            var newToken = _jwtService.GenerateToken(result.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Update refresh token
            await _mediator.Send(new UpdateRefreshTokenCommand
            {
                UserId = result.User.Id.Value,
                OldRefreshToken = request.RefreshToken,
                NewRefreshToken = newRefreshToken
            });

            return Ok(new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return Unauthorized(new { error = "Token refresh failed" });
        }
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <returns>Success response</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerOperation(
        Summary = "User logout",
        Description = "Logs out a user by invalidating their refresh token",
        OperationId = "Logout",
        Tags = new[] { "Authentication" }
    )]
    public async Task<ActionResult> Logout(LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await _mediator.Send(new InvalidateRefreshTokenCommand
            {
                UserId = userId,
                RefreshToken = request.RefreshToken
            });

            _logger.LogInformation("User logged out: {UserId}", userId);
        }

        return Ok(new { message = "Logout successful" });
    }
}
```

---

## 🛡️ Authorization Implementation

### Step 4: Role-Based Authorization

#### Authorization Attributes
```csharp
// TaskFlow.API/Attributes/RequirePermissionAttribute.cs
namespace TaskFlow.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

// TaskFlow.API/Attributes/RequireRoleAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute
{
    public string Role { get; }

    public RequireRoleAttribute(string role)
    {
        Role = role;
    }
}
```

#### Authorization Handler
```csharp
// TaskFlow.API/Handlers/PermissionAuthorizationHandler.cs
namespace TaskFlow.API.Handlers;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        var permissions = context.User.FindFirst("permissions")?.Value;
        
        if (string.IsNullOrEmpty(permissions))
        {
            return Task.CompletedTask;
        }

        var userPermissions = permissions.Split(',');
        
        if (userPermissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
```

#### Enhanced Tasks Controller with Authorization
```csharp
// TaskFlow.API/Controllers/V2/TasksController.cs
namespace TaskFlow.API.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize] // Require authentication for all endpoints
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new task (requires task:create permission)
    /// </summary>
    [HttpPost]
    [RequirePermission("task:create")]
    [ProducesResponseType(typeof(CreateTaskV2Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Create a new task (v2)",
        Description = "Creates a new task with enhanced features. Requires task:create permission.",
        OperationId = "CreateTaskV2",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<CreateTaskV2Response>> CreateTask(CreateTaskV2Command command)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        command.CreatedBy = userId;

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = result.TaskId, version = "2.0" }, result);
    }

    /// <summary>
    /// Retrieves a task by ID (requires task:read permission)
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("task:read")]
    [ProducesResponseType(typeof(GetTaskV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Get task by ID (v2)",
        Description = "Retrieves a task with enhanced details. Requires task:read permission.",
        OperationId = "GetTaskV2",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<GetTaskV2Response>> GetTask(
        string id, 
        [FromQuery] bool includeComments = false,
        [FromQuery] bool includeHistory = false)
    {
        var query = new GetTaskV2Query 
        { 
            TaskId = id,
            IncludeComments = includeComments,
            IncludeHistory = includeHistory
        };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Assigns a task to a user (requires task:assign permission)
    /// </summary>
    [HttpPost("{id}/assign")]
    [RequirePermission("task:assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Assign task to user",
        Description = "Assigns a task to a specific user. Requires task:assign permission.",
        OperationId = "AssignTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult> AssignTask(string id, AssignTaskCommand command)
    {
        command.TaskId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a task (requires task:delete permission and admin role)
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("task:delete")]
    [RequireRole("admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [SwaggerOperation(
        Summary = "Delete a task",
        Description = "Permanently deletes a task. Requires task:delete permission and admin role.",
        OperationId = "DeleteTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult> DeleteTask(string id)
    {
        var command = new DeleteTaskCommand { TaskId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
```

---

## 🔒 Security Best Practices

### Step 5: Input Validation

#### FluentValidation Validators
```csharp
// TaskFlow.Application/Validators/CreateTaskV2CommandValidator.cs
namespace TaskFlow.Application.Validators;

public class CreateTaskV2CommandValidator : AbstractValidator<CreateTaskV2Command>
{
    public CreateTaskV2CommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-zA-Z0-9\s\-_.,!?()]+$")
            .WithMessage("Title contains invalid characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000)
            .Matches(@"^[a-zA-Z0-9\s\-_.,!?()]+$")
            .WithMessage("Description contains invalid characters");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Project ID contains invalid characters");

        RuleFor(x => x.Priority)
            .IsInEnum()
            .WithMessage("Invalid priority value");

        RuleFor(x => x.EstimatedHours)
            .InclusiveBetween(1, 1000)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be between 1 and 1000");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= 10)
            .WithMessage("Maximum 10 tags allowed");

        RuleForEach(x => x.Tags)
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Tag contains invalid characters");
    }
}
```

#### Security Headers Middleware
```csharp
// TaskFlow.API/Middleware/SecurityHeadersMiddleware.cs
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
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        // Add Content Security Policy
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';");

        await _next(context);
    }
}
```

### Step 6: Rate Limiting

#### Rate Limiting Configuration
```csharp
// Program.cs
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("authenticated", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromHour(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : 60
        }, token);
    };
});

var app = builder.Build();

// Enable rate limiting
app.UseRateLimiter();
```

#### Rate Limited Controller
```csharp
// TaskFlow.API/Controllers/RateLimitedController.cs
namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("authenticated")]
public class RateLimitedController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("Rate limited endpoint");
    }
}
```

---

## 🧪 Security Testing

### Step 7: Security Test Implementation

#### Authentication Tests
```csharp
// TaskFlow.Tests/Security/AuthenticationTests.cs
namespace TaskFlow.Tests.Security;

public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v2/tasks/task_001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetValidToken(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/v2/tasks/task_001");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    private async Task<string> GetValidToken(HttpClient client)
    {
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }
}
```

#### Authorization Tests
```csharp
// TaskFlow.Tests/Security/AuthorizationTests.cs
namespace TaskFlow.Tests.Security;

public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTask_WithoutPermission_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenWithRole(client, "user"); // User without task:create permission
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createTaskRequest = new CreateTaskV2Command
        {
            Title = "Test Task",
            Description = "Test Description",
            ProjectId = "project_123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v2/tasks", createTaskRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTask_WithoutAdminRole_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateClient();
        var token = await GetTokenWithRole(client, "user"); // User without admin role
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync("/api/v2/tasks/task_001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<string> GetTokenWithRole(HttpClient client, string role)
    {
        // Implementation to get token with specific role
        // This would depend on your test data setup
        throw new NotImplementedException();
    }
}
```

---

## 📋 Security Configuration

### Step 8: Program.cs Security Configuration
```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

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

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = "Authentication failed" });
            return context.Response.WriteAsync(result);
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { error = "Authentication required" });
            return context.Response.WriteAsync(result);
        }
    };
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("RequireTaskCreatePermission", policy =>
        policy.RequireClaim("permissions", "task:create"));

    options.AddPolicy("RequireTaskReadPermission", policy =>
        policy.RequireClaim("permissions", "task:read"));

    options.AddPolicy("RequireTaskUpdatePermission", policy =>
        policy.RequireClaim("permissions", "task:update"));

    options.AddPolicy("RequireTaskDeletePermission", policy =>
        policy.RequireClaim("permissions", "task:delete"));
});

// Add services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var app = builder.Build();

// Configure middleware
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
```

---

## 🎯 Key Learning Points

### Authentication Best Practices
1. **JWT Tokens**: Secure, stateless authentication
2. **Refresh Tokens**: Extend session without re-authentication
3. **Token Validation**: Server-side validation of all tokens
4. **Secure Storage**: Never store sensitive data in tokens

### Authorization Best Practices
1. **Role-Based Access Control (RBAC)**: Assign permissions to roles
2. **Permission-Based Authorization**: Fine-grained access control
3. **Principle of Least Privilege**: Grant minimum required permissions
4. **Regular Audits**: Review and update permissions regularly

### Security Best Practices
1. **Input Validation**: Validate all user inputs
2. **Output Encoding**: Prevent XSS attacks
3. **Security Headers**: Protect against common attacks
4. **Rate Limiting**: Prevent abuse and DoS attacks
5. **HTTPS Only**: Encrypt all communications

### Common Security Vulnerabilities
1. **SQL Injection**: Use parameterized queries
2. **XSS**: Encode output and validate input
3. **CSRF**: Use anti-forgery tokens
4. **Authentication Bypass**: Validate all requests
5. **Information Disclosure**: Don't expose sensitive data

---

**Implementation Success**: Students can implement comprehensive security foundations including JWT authentication, role-based authorization, input validation, and security best practices for production-ready .NET APIs. 