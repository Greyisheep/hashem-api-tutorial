# Authentication Workshop - Hands-on Implementation

## 🎯 Workshop Objectives
- Implement JWT authentication hands-on with the actual TaskFlow API
- Understand authentication vs authorization in .NET
- Practice secure API key implementation
- Prepare for OAuth 2.0 take-home assignment
- Learn Domain-Driven Design security patterns

## ⏰ Workshop Timing: 90 minutes

---

## 🚀 Setup (5 minutes)

### Prerequisites Check
- [ ] .NET 8 SDK installed
- [ ] TaskFlow API project ready (`taskflow-api-dotnet`)
- [ ] Postman collections imported
- [ ] Database schema from morning session
- [ ] Docker running (for PostgreSQL)

### Quick Authentication Overview
**Question**: "What's the difference between authentication and authorization?"

<details>
<summary>Click to reveal answer</summary>

- **Authentication**: "Who are you?" (Identity verification)
  - Validates user credentials
  - Issues JWT tokens
  - Establishes user identity

- **Authorization**: "What can you do?" (Permission verification)
  - Checks user roles and permissions
  - Controls access to resources
  - Enforces business rules
</details>

---

## 🔐 JWT Implementation (45 minutes)

### Step 1: Review Existing JWT Implementation (10 minutes)

**Current Implementation Analysis**: Let's examine the actual TaskFlow API authentication

#### Reference: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/AuthController.cs`

<details>
<summary>Current AuthController Implementation</summary>

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.API/Controllers/AuthController.cs
[HttpPost("login")]
public ActionResult<ApiResponse<object>> Login([FromBody] LoginRequest request)
{
    try
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Simple authentication for demo purposes
        // In production, this would validate against a database
        if (request.Username == "admin@taskflow.com" && request.Password == "admin123")
        {
            var token = GenerateJwtToken(request.Username, "Admin");
            
            var response = new
            {
                token = token,
                refreshToken = Guid.NewGuid().ToString(),
                expiresIn = 3600,
                user = new
                {
                    username = request.Username,
                    role = "Admin"
                }
            };

            return Ok(ApiResponse.SuccessResponse(response, "Login successful"));
        }

        return Unauthorized(ApiResponse.ErrorResponse("INVALID_CREDENTIALS", "Invalid username or password"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
        return StatusCode(500, ApiResponse.ErrorResponse("LOGIN_ERROR", "An error occurred during login"));
    }
}
```

**Key Observations**:
- Uses envelope pattern (`ApiResponse<T>`)
- Includes comprehensive logging
- Has proper error handling
- Returns JWT token with refresh token
- Uses hardcoded credentials (demo only)

</details>

#### Current JWT Configuration
<details>
<summary>appsettings.json JWT Settings</summary>

```json
// Reference: taskflow-api-dotnet/src/TaskFlow.API/appsettings.json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-256-bits-for-development-only",
    "Issuer": "TaskFlow-API",
    "Audience": "TaskFlow-Users",
    "ExpirationMinutes": 60
  }
}
```

**Security Note**: The current secret key is for development only. In production, use a strong, randomly generated key.
</details>

### Step 2: Enhance JWT Configuration (15 minutes)

**Hands-on**: Improve the JWT configuration for production readiness

#### Update appsettings.json
```json
// Reference: taskflow-api-dotnet/src/TaskFlow.API/appsettings.json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-256-bits-for-development-only",
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

#### Program.cs JWT Configuration
<details>
<summary>Current JWT Setup in Program.cs</summary>

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.API/Program.cs
// Add Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    });

// Add Authorization
builder.Services.AddAuthorization();
```

**Security Features**:
- ✅ Validates issuer signing key
- ✅ Validates issuer and audience
- ✅ Validates token lifetime
- ✅ Zero clock skew for strict timing
</details>

### Step 3: Create JWT Service (15 minutes)

**Code-along**: Create a dedicated JWT service following DDD principles

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.Infrastructure/Services/JwtService.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskFlow.Domain.Entities;

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
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
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

            return tokenHandler.ValidateToken(token, validationParameters, out _);
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

**Question**: Why do we use a dedicated JWT service instead of generating tokens in the controller?

<details>
<summary>Click to reveal answer</summary>

1. **Separation of Concerns**: Authentication logic is separated from HTTP handling
2. **Reusability**: The service can be used across different controllers
3. **Testability**: Easier to unit test authentication logic
4. **Maintainability**: Centralized token generation and validation
5. **DDD Principles**: Follows domain service patterns
</details>

### Step 4: Test JWT Authentication (5 minutes)

**Hands-on**: Test with Postman using the actual TaskFlow API

```bash
# Start the TaskFlow API
cd taskflow-api-dotnet
./start.sh

# Login to get token
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin@taskflow.com",
  "password": "admin123"
}

# Expected Response (envelope pattern):
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc123...",
    "expiresIn": 3600,
    "user": {
      "username": "admin@taskflow.com",
      "role": "Admin"
    }
  }
}

# Use token in Authorization header
GET http://localhost:5000/api/tasks
Authorization: Bearer <your-jwt-token>
```

**Question**: What happens if you try to access a protected endpoint without a token?

<details>
<summary>Click to reveal answer</summary>

You'll receive a 401 Unauthorized response because:
1. The `[Authorize]` attribute requires authentication
2. No valid JWT token is provided
3. The JWT middleware rejects the request
4. The envelope pattern returns an error response
</details>

---

## 🔑 API Keys Implementation (25 minutes)

### Step 1: API Key Model (5 minutes)

**Hands-on**: Create API key entity following DDD principles

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.Domain/Entities/ApiKey.cs
using TaskFlow.Domain.Common;
using TaskFlow.Domain.DomainEvents;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

public class ApiKey : AggregateRoot
{
    public ApiKeyId Id { get; private set; }
    public string Key { get; private set; }
    public string Name { get; private set; }
    public UserId UserId { get; private set; }
    public List<string> Permissions { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private ApiKey() 
    {
        Id = default!;
        Key = default!;
        Name = default!;
        UserId = default!;
        Permissions = new List<string>();
    }
    
    public ApiKey(string name, UserId userId, List<string> permissions, DateTime expiresAt)
    {
        Id = ApiKeyId.New();
        Key = GenerateApiKey();
        Name = name;
        UserId = userId;
        Permissions = permissions ?? new List<string>();
        ExpiresAt = expiresAt;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ApiKeyCreatedEvent(Id.Value, UserId.Value));
    }
    
    private string GenerateApiKey() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    
    public void Revoke()
    {
        if (!IsActive)
            throw new DomainException("API key is already revoked");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ApiKeyRevokedEvent(Id.Value));
    }

    public void ExtendExpiration(DateTime newExpiration)
    {
        if (newExpiration <= DateTime.UtcNow)
            throw new DomainException("New expiration must be in the future");

        ExpiresAt = newExpiration;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ApiKeyExtendedEvent(Id.Value, newExpiration));
    }
}

// Create: taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/ApiKeyId.cs
namespace TaskFlow.Domain.ValueObjects;

public class ApiKeyId : ValueObject<string>
{
    private ApiKeyId(string value) : base(value) { }
    public static ApiKeyId New() => new(Guid.NewGuid().ToString());
    public static ApiKeyId From(string value) => new(value);
}
```

### Step 2: API Key Authentication (10 minutes)

**Hands-on**: Implement API key authentication middleware

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.API/Middleware/ApiKeyMiddleware.cs
using TaskFlow.Application.Interfaces;

namespace TaskFlow.API.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    
    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            await _next(context);
            return;
        }
        
        var apiKey = apiKeyHeader.ToString();
        
        // In production, validate against database
        // For demo, use a simple validation
        if (IsValidApiKey(apiKey))
        {
            // Add user info to context
            context.Items["ApiKey"] = apiKey;
            context.Items["UserId"] = "demo-user-id";
            context.Items["Permissions"] = new[] { "read:tasks", "write:tasks" };
            
            _logger.LogInformation("API key authentication successful");
        }
        else
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var errorResponse = new
            {
                success = false,
                message = "Invalid API key",
                errorCode = "INVALID_API_KEY"
            };
            await context.Response.WriteAsJsonAsync(errorResponse);
            return;
        }
        
        await _next(context);
    }

    private bool IsValidApiKey(string apiKey)
    {
        // Demo validation - in production, check against database
        return !string.IsNullOrEmpty(apiKey) && apiKey.Length >= 32;
    }
}

// Extension method for easy registration
public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
```

### Step 3: Test API Keys (10 minutes)

**Hands-on**: Test API key authentication

```bash
# Use API key (demo key for testing)
GET http://localhost:5000/api/tasks
X-API-Key: demo-api-key-12345678901234567890123456789012

# Expected Response:
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": [
    {
      "id": "task_001",
      "title": "Implement Authentication",
      "description": "Implement JWT-based authentication",
      "status": "InProgress"
    }
  ]
}
```

**Question**: What are the advantages and disadvantages of API keys vs JWT tokens?

<details>
<summary>Click to reveal answer</summary>

**API Keys Advantages**:
- Simple to implement
- No expiration management
- Good for server-to-server communication
- Stateless

**API Keys Disadvantages**:
- Less secure (no expiration)
- Harder to revoke
- No user context
- Limited to simple authentication

**JWT Tokens Advantages**:
- Include user context and claims
- Automatic expiration
- Can be revoked via refresh tokens
- More secure

**JWT Tokens Disadvantages**:
- More complex to implement
- Larger payload
- Requires token management
</details>

---

## 🎯 Authorization Implementation (15 minutes)

### Step 1: Role-Based Authorization (10 minutes)

**Hands-on**: Add authorization attributes to the actual TasksController

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.API/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Requires authentication for all endpoints
[SwaggerTag("Task management operations")]
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
    [Authorize(Roles = "Admin,ProjectManager")] // Role-based authorization
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(CreateTaskCommand request)
    {
        try
        {
            _logger.LogInformation("Creating task: {Title}", request.Title);

            var result = await _mediator.Send(request);

            _logger.LogInformation("Task created successfully: {TaskId}", result.Id);

            var response = ApiResponse<TaskDto>.SuccessResponse(result, "Task created successfully");
            return CreatedAtAction(nameof(GetTask), new { id = result.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task: {Title}", request.Title);
            return StatusCode(500, ApiResponse<TaskDto>.ErrorResponse("INTERNAL_ERROR", "An error occurred while creating the task"));
        }
    }

    /// <summary>
    /// Retrieves a task by ID (requires task:read permission)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,ProjectManager,Developer")] // Multiple roles
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving task: {TaskId}", id);

            var query = new GetTaskQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", id);
                return NotFound(ApiResponse<TaskDto>.ErrorResponse("TASK_NOT_FOUND", $"Task with ID {id} not found"));
            }

            _logger.LogInformation("Task retrieved successfully: {TaskId}", id);
            return Ok(ApiResponse<TaskDto>.SuccessResponse(result, "Task retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task: {TaskId}", id);
            return StatusCode(500, ApiResponse<TaskDto>.ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving the task"));
        }
    }

    /// <summary>
    /// Gets all tasks (requires task:read permission)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ProjectManager,Developer")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetAllTasks()
    {
        try
        {
            _logger.LogInformation("Retrieving all tasks");

            var query = new GetAllTasksQuery();
            var tasks = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<TaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tasks");
            return StatusCode(500, ApiResponse<IEnumerable<TaskDto>>.ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving tasks"));
        }
    }
}
```

### Step 2: Permission-Based Authorization (5 minutes)

**Hands-on**: Custom permission attributes

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.API/Attributes/RequirePermissionAttribute.cs
namespace TaskFlow.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base()
    {
        Policy = $"Permission_{permission}";
    }
}

// Usage example:
[HttpGet]
[RequirePermission("task:read")]
public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetAllTasks()
{
    // Implementation
}
```

**Question**: What's the difference between role-based and permission-based authorization?

<details>
<summary>Click to reveal answer</summary>

**Role-Based Authorization**:
- Groups permissions into roles (Admin, Developer, etc.)
- Simpler to manage
- Less granular control
- Good for simple applications

**Permission-Based Authorization**:
- Individual permissions (task:read, task:write, etc.)
- More granular control
- More complex to manage
- Better for complex applications
- Can combine with roles for flexibility
</details>

---

## 🎯 Success Criteria

### Excellent Implementation (All of these):
- [ ] JWT authentication working with actual TaskFlow API
- [ ] API key authentication working
- [ ] Role-based authorization working
- [ ] Permission-based authorization working
- [ ] All endpoints properly secured
- [ ] Understanding of envelope pattern
- [ ] Can explain DDD security patterns

### Good Implementation (Most of these):
- [ ] JWT authentication working
- [ ] Basic authorization working
- [ ] Some endpoints secured
- [ ] Understanding of auth concepts
- [ ] Can explain implementation
- [ ] Familiar with envelope pattern

### Needs Improvement (Few of these):
- [ ] Authentication not working
- [ ] No authorization implemented
- [ ] Security vulnerabilities
- [ ] Poor understanding of concepts
- [ ] Cannot explain implementation

---

## 📝 Facilitator Notes

### Common Issues to Watch For:
- **JWT secret too short**: Must be at least 256 bits
- **Missing token validation**: Always validate tokens
- **No expiration**: Always set token expiration
- **Hardcoded secrets**: Use configuration
- **Missing error handling**: Handle auth failures gracefully
- **Envelope pattern**: Ensure consistent response format

### Questions to Ask:
- "How do you handle token refresh?"
- "What happens when a user's role changes?"
- "How do you revoke API keys?"
- "What about rate limiting?"
- "How does the envelope pattern improve API consistency?"

### Energy Management:
- **Celebrate working auth** - "Great! You're authenticated!"
- **Help debug issues** - "Let's check the token..."
- **Build security awareness** - "Think like an attacker"
- **Encourage questions** - "Security is complex!"

### Reference the Actual Code:
- **Auth Controller**: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/AuthController.cs`
- **Tasks Controller**: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/TasksController.cs`
- **Program.cs**: `taskflow-api-dotnet/src/TaskFlow.API/Program.cs`
- **User Entity**: `taskflow-api-dotnet/src/TaskFlow.Domain/Entities/User.cs`
- **appsettings.json**: `taskflow-api-dotnet/src/TaskFlow.API/appsettings.json`

---

## 🚀 Take-Home Assignment: OAuth 2.0

### OAuth 2.0 Implementation Guide
**Task**: Implement OAuth 2.0 with Google/GitHub

#### Resources Provided:
- OAuth 2.0 flow diagrams
- Google OAuth 2.0 setup guide
- GitHub OAuth 2.0 setup guide
- OAuth 2.0 implementation examples

#### Implementation Steps:
1. **Set up OAuth provider** (Google/GitHub)
2. **Implement authorization code flow**
3. **Handle token exchange**
4. **Create user account from OAuth data**
5. **Generate JWT token for your API**

#### Success Criteria:
- [ ] OAuth 2.0 flow working end-to-end
- [ ] User creation/update from OAuth data
- [ ] JWT token generation
- [ ] Error handling and validation
- [ ] Clean, maintainable code
- [ ] Follows envelope pattern

### Day 3 Preview:
"Tomorrow we'll add advanced security features and deploy to production!" 
