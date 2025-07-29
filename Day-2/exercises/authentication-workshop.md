# Authentication Workshop - Hands-on Implementation

## 🎯 Workshop Objectives
- Implement JWT authentication hands-on
- Understand authentication vs authorization
- Practice secure API key implementation
- Prepare for OAuth 2.0 take-home assignment

## ⏰ Workshop Timing: 90 minutes

---

## 🚀 Setup (5 minutes)

### Prerequisites Check
- [ ] .NET 8 SDK installed
- [ ] TaskFlow API project ready
- [ ] Postman collections imported
- [ ] Database schema from morning session

### Quick Authentication Overview
**Question**: "What's the difference between authentication and authorization?"
- **Authentication**: "Who are you?" (Identity)
- **Authorization**: "What can you do?" (Permissions)

---

## 🔐 JWT Implementation (45 minutes)

### Step 1: Review Existing JWT Implementation (10 minutes)
**Show**: Current JWT implementation in TaskFlow API

#### Reference: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/AuthController.cs`

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

private string GenerateJwtToken(string username, string role)
{
    var jwtSettings = _configuration.GetSection("JwtSettings");
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
    var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, username),
        new(ClaimTypes.Name, username),
        new(ClaimTypes.Role, role)
    };

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### Step 2: Enhance JWT Configuration (15 minutes)
**Hands-on**: Improve the JWT configuration

#### Update appsettings.json
```json
// Reference: taskflow-api-dotnet/src/TaskFlow.API/appsettings.json
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

### Step 3: Create JWT Service (15 minutes)
**Code-along**: Create a dedicated JWT service

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.Infrastructure/Services/JwtService.cs
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
            new(ClaimTypes.Name, $"{user.FirstName.Value} {user.LastName.Value}"),
            new(ClaimTypes.Role, user.Role.Value)
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
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
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
}
```

### Step 4: Test JWT Authentication (5 minutes)
**Hands-on**: Test with Postman

```bash
# Login to get token
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin@taskflow.com",
  "password": "admin123"
}

# Use token in Authorization header
GET http://localhost:5000/api/tasks
Authorization: Bearer <your-jwt-token>
```

---

## 🔑 API Keys Implementation (25 minutes)

### Step 1: API Key Model (5 minutes)
**Hands-on**: Create API key entity

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.Domain/Entities/ApiKey.cs
public class ApiKey : AggregateRoot
{
    public ApiKeyId Id { get; private set; }
    public string Key { get; private set; }
    public string Name { get; private set; }
    public UserId UserId { get; private set; }
    public List<string> Permissions { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    
    public ApiKey(string name, UserId userId, List<string> permissions, DateTime expiresAt)
    {
        Id = ApiKeyId.New();
        Key = GenerateApiKey();
        Name = name;
        UserId = userId;
        Permissions = permissions;
        ExpiresAt = expiresAt;
        IsActive = true;
    }
    
    private string GenerateApiKey() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    
    public void Revoke()
    {
        IsActive = false;
        AddDomainEvent(new ApiKeyRevokedEvent(Id));
    }
}

// Create: taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/ApiKeyId.cs
public class ApiKeyId : ValueObject<string>
{
    private ApiKeyId(string value) : base(value) { }
    public static ApiKeyId New() => new(Guid.NewGuid().ToString());
    public static ApiKeyId From(string value) => new(value);
}
```

### Step 2: API Key Authentication (10 minutes)
**Hands-on**: Implement API key authentication

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.API/Middleware/ApiKeyMiddleware.cs
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyRepository _apiKeyRepository;
    
    public ApiKeyMiddleware(RequestDelegate next, IApiKeyRepository apiKeyRepository)
    {
        _next = next;
        _apiKeyRepository = apiKeyRepository;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            await _next(context);
            return;
        }
        
        var apiKey = await _apiKeyRepository.GetByKeyAsync(apiKeyHeader);
        
        if (apiKey == null || !apiKey.IsActive || apiKey.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }
        
        // Add user info to context
        context.Items["UserId"] = apiKey.UserId;
        context.Items["Permissions"] = apiKey.Permissions;
        
        await _next(context);
    }
}
```

### Step 3: Test API Keys (10 minutes)
**Hands-on**: Test API key authentication

```bash
# Create API key (admin only)
POST http://localhost:5000/api/auth/api-keys
Authorization: Bearer <admin-jwt-token>
Content-Type: application/json

{
  "name": "My API Key",
  "permissions": ["read:tasks", "write:tasks"],
  "expiresAt": "2024-12-31T23:59:59Z"
}

# Use API key
GET http://localhost:5000/api/tasks
X-API-Key: <your-api-key>
```

---

## 🎯 Authorization Implementation (15 minutes)

### Step 1: Role-Based Authorization (10 minutes)
**Hands-on**: Add authorization attributes

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.API/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class TasksController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin,project_manager,developer")] // Role-based
    public async Task<ActionResult<List<TaskDto>>> GetTasks()
    {
        // Implementation
    }
    
    [HttpPost]
    [Authorize(Roles = "admin,project_manager")] // Only managers can create
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
    {
        // Implementation
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")] // Only admins can delete
    public async Task<ActionResult> DeleteTask(string id)
    {
        // Implementation
    }
}
```

### Step 2: Permission-Based Authorization (5 minutes)
**Hands-on**: Custom permission attributes

```csharp
// Create: taskflow-api-dotnet/src/TaskFlow.API/Attributes/RequirePermissionAttribute.cs
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base()
    {
        Policy = $"Permission_{permission}";
    }
}

// Usage
[HttpGet]
[RequirePermission("read:tasks")]
public async Task<ActionResult<List<TaskDto>>> GetTasks()
{
    // Implementation
}
```

---

## 🎯 Success Criteria

### Excellent Implementation (All of these):
- [ ] JWT authentication working
- [ ] API key authentication working
- [ ] Role-based authorization working
- [ ] Permission-based authorization working
- [ ] All endpoints properly secured

### Good Implementation (Most of these):
- [ ] JWT authentication working
- [ ] Basic authorization working
- [ ] Some endpoints secured
- [ ] Understanding of auth concepts
- [ ] Can explain implementation

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

### Questions to Ask:
- "How do you handle token refresh?"
- "What happens when a user's role changes?"
- "How do you revoke API keys?"
- "What about rate limiting?"

### Energy Management:
- **Celebrate working auth** - "Great! You're authenticated!"
- **Help debug issues** - "Let's check the token..."
- **Build security awareness** - "Think like an attacker"
- **Encourage questions** - "Security is complex!"

### Reference the Actual Code:
- **Auth Controller**: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/AuthController.cs`
- **Tasks Controller**: `taskflow-api-dotnet/src/TaskFlow.API/Controllers/TasksController.cs`
- **Domain Entities**: `taskflow-api-dotnet/src/TaskFlow.Domain/Entities/`
- **Value Objects**: `taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/`

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

### Day 3 Preview:
"Tomorrow we'll add advanced security features and deploy to production!" 