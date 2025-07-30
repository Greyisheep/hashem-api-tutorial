# WebSocket Security Demo - TaskFlow API

## 🎯 Overview
This demo shows how to implement secure WebSocket connections in the TaskFlow API with JWT authentication, proper authorization, and real-time task updates.

## 🔐 WebSocket Security Challenges

### Common Vulnerabilities
- **No Authentication**: Anyone can connect to WebSocket
- **No Authorization**: Users can access any data
- **Token Exposure**: JWT tokens in URL parameters
- **No Rate Limiting**: WebSocket connections not limited
- **No Validation**: Messages not validated

### Security Requirements
- **Authentication**: Verify user identity
- **Authorization**: Check user permissions
- **Message Validation**: Validate all messages
- **Rate Limiting**: Limit connection attempts
- **Secure Token Handling**: Tokens in headers, not URLs

## 🛠️ Implementation

### Step 1: WebSocket Hub with Authentication

#### 1.1 Create Secure WebSocket Hub

```csharp
// src/TaskFlow.API/Hubs/SecureTaskHub.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskFlow.API.Hubs;

[Authorize]
public class SecureTaskHub : Hub
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SecureTaskHub> _logger;
    private readonly IConfiguration _configuration;

    public SecureTaskHub(
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        ILogger<SecureTaskHub> logger,
        IConfiguration configuration)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("Unauthorized WebSocket connection attempt");
            Context.Abort();
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            _logger.LogWarning("User not found for WebSocket connection: {UserId}", userId);
            Context.Abort();
            return;
        }

        // Add user to groups based on their projects
        var userProjects = await _taskRepository.GetUserProjectsAsync(userId.Value);
        foreach (var project in userProjects)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{project.Id}");
        }

        // Add user to their personal group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        _logger.LogInformation("User {UserId} connected to WebSocket", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        if (userId != null)
        {
            _logger.LogInformation("User {UserId} disconnected from WebSocket", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    [Authorize]
    public async Task JoinProject(string projectId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        // Verify user has access to this project
        var hasAccess = await _taskRepository.UserHasProjectAccessAsync(userId.Value, int.Parse(projectId));
        if (!hasAccess)
        {
            _logger.LogWarning("User {UserId} attempted to access unauthorized project {ProjectId}", userId, projectId);
            await Clients.Caller.SendAsync("Error", "Access denied");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("JoinedProject", projectId);
    }

    [Authorize]
    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("LeftProject", projectId);
    }

    [Authorize]
    public async Task UpdateTaskStatus(string taskId, string newStatus)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        // Validate input
        if (!Enum.TryParse<TaskStatus>(newStatus, out var status))
        {
            await Clients.Caller.SendAsync("Error", "Invalid status");
            return;
        }

        // Verify user can update this task
        var task = await _taskRepository.GetByIdAsync(int.Parse(taskId));
        if (task == null || task.AssignedUserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to update unauthorized task {TaskId}", userId, taskId);
            await Clients.Caller.SendAsync("Error", "Access denied");
            return;
        }

        // Update task status
        task.UpdateStatus(status);
        await _taskRepository.UpdateAsync(task);

        // Notify project members
        await Clients.Group($"project_{task.ProjectId}").SendAsync("TaskStatusUpdated", new
        {
            TaskId = taskId,
            NewStatus = newStatus,
            UpdatedBy = userId,
            UpdatedAt = DateTime.UtcNow
        });

        _logger.LogInformation("Task {TaskId} status updated to {Status} by user {UserId}", taskId, newStatus, userId);
    }

    private Guid? GetCurrentUserId()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
```

### Step 2: JWT Authentication for SignalR

#### 2.1 Create JWT Authentication Handler

```csharp
// src/TaskFlow.API/Authentication/JwtSignalRAuthenticationHandler.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace TaskFlow.API.Authentication;

public class JwtSignalRAuthenticationHandler : IUserIdProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtSignalRAuthenticationHandler> _logger;

    public JwtSignalRAuthenticationHandler(IConfiguration configuration, ILogger<JwtSignalRAuthenticationHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string? GetUserId(HubConnectionContext connection)
    {
        var token = GetTokenFromConnection(connection);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Authentication:Jwt:SecretKey"]!);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Authentication:Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Authentication:Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("No user ID found in JWT token");
                return null;
            }

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token for WebSocket connection");
            return null;
        }
    }

    private string? GetTokenFromConnection(HubConnectionContext connection)
    {
        // Try to get token from query string
        if (connection.GetHttpContext()?.Request.Query.TryGetValue("access_token", out var token) == true)
        {
            return token.ToString();
        }

        // Try to get token from headers
        var authHeader = connection.GetHttpContext()?.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length);
        }

        return null;
    }
}
```

### Step 3: Configure SignalR with Authentication

#### 3.1 Update Program.cs

```csharp
// Add SignalR with authentication
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false; // Disable in production
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB limit
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Configure JWT authentication for SignalR
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
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

        // Configure SignalR authentication
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

// Add SignalR user ID provider
builder.Services.AddSingleton<IUserIdProvider, JwtSignalRAuthenticationHandler>();
```

### Step 4: WebSocket Rate Limiting

#### 4.1 Create WebSocket Rate Limiting

```csharp
// src/TaskFlow.API/Middleware/WebSocketRateLimitingMiddleware.cs
public class WebSocketRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WebSocketRateLimitingMiddleware> _logger;

    public WebSocketRateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<WebSocketRateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey = $"websocket_connection:{ip}";
            
            var connectionCount = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return Task.FromResult(0);
            });

            if (connectionCount >= 10) // Max 10 connections per IP per minute
            {
                _logger.LogWarning("WebSocket rate limit exceeded for IP: {IP}", ip);
                context.Response.StatusCode = 429; // Too Many Requests
                return;
            }

            _cache.Set(cacheKey, connectionCount + 1, TimeSpan.FromMinutes(1));
        }

        await _next(context);
    }
}
```

### Step 5: Real-time Task Updates

#### 5.1 Create Task Update Service

```csharp
// src/TaskFlow.API/Services/TaskUpdateService.cs
public interface ITaskUpdateService
{
    Task NotifyTaskCreatedAsync(TaskFlow.Domain.Entities.Task task);
    Task NotifyTaskUpdatedAsync(TaskFlow.Domain.Entities.Task task);
    Task NotifyTaskStatusChangedAsync(TaskFlow.Domain.Entities.Task task, string oldStatus);
    Task NotifyTaskAssignedAsync(TaskFlow.Domain.Entities.Task task, Guid? previousAssignee);
}

public class TaskUpdateService : ITaskUpdateService
{
    private readonly IHubContext<SecureTaskHub> _hubContext;
    private readonly ILogger<TaskUpdateService> _logger;

    public TaskUpdateService(IHubContext<SecureTaskHub> hubContext, ILogger<TaskUpdateService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyTaskCreatedAsync(TaskFlow.Domain.Entities.Task task)
    {
        var notification = new
        {
            Type = "TaskCreated",
            TaskId = task.Id.Value,
            Title = task.Title.Value,
            ProjectId = task.ProjectId.Value,
            AssignedUserId = task.AssignedUserId,
            Status = task.Status.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{task.ProjectId.Value}").SendAsync("TaskCreated", notification);
        _logger.LogInformation("Task {TaskId} creation notified to project {ProjectId}", task.Id, task.ProjectId);
    }

    public async Task NotifyTaskUpdatedAsync(TaskFlow.Domain.Entities.Task task)
    {
        var notification = new
        {
            Type = "TaskUpdated",
            TaskId = task.Id.Value,
            Title = task.Title.Value,
            ProjectId = task.ProjectId.Value,
            AssignedUserId = task.AssignedUserId,
            Status = task.Status.ToString(),
            UpdatedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{task.ProjectId.Value}").SendAsync("TaskUpdated", notification);
        _logger.LogInformation("Task {TaskId} update notified to project {ProjectId}", task.Id, task.ProjectId);
    }

    public async Task NotifyTaskStatusChangedAsync(TaskFlow.Domain.Entities.Task task, string oldStatus)
    {
        var notification = new
        {
            Type = "TaskStatusChanged",
            TaskId = task.Id.Value,
            Title = task.Title.Value,
            ProjectId = task.ProjectId.Value,
            OldStatus = oldStatus,
            NewStatus = task.Status.ToString(),
            ChangedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{task.ProjectId.Value}").SendAsync("TaskStatusChanged", notification);
        
        // Notify the assigned user specifically
        if (task.AssignedUserId.HasValue)
        {
            await _hubContext.Clients.Group($"user_{task.AssignedUserId.Value}").SendAsync("TaskStatusChanged", notification);
        }

        _logger.LogInformation("Task {TaskId} status change from {OldStatus} to {NewStatus} notified", 
            task.Id, oldStatus, task.Status);
    }

    public async Task NotifyTaskAssignedAsync(TaskFlow.Domain.Entities.Task task, Guid? previousAssignee)
    {
        var notification = new
        {
            Type = "TaskAssigned",
            TaskId = task.Id.Value,
            Title = task.Title.Value,
            ProjectId = task.ProjectId.Value,
            PreviousAssignee = previousAssignee,
            NewAssignee = task.AssignedUserId,
            AssignedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"project_{task.ProjectId.Value}").SendAsync("TaskAssigned", notification);
        
        // Notify the new assignee specifically
        if (task.AssignedUserId.HasValue)
        {
            await _hubContext.Clients.Group($"user_{task.AssignedUserId.Value}").SendAsync("TaskAssigned", notification);
        }

        _logger.LogInformation("Task {TaskId} assigned to user {UserId}", task.Id, task.AssignedUserId);
    }
}
```

### Step 6: Frontend WebSocket Client

#### 6.1 JavaScript WebSocket Client

```javascript
// Frontend WebSocket client with authentication
class SecureTaskWebSocket {
    constructor(token) {
        this.token = token;
        this.connection = null;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }

    connect() {
        const url = `wss://localhost:7001/hubs/securetaskhub?access_token=${this.token}`;
        
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(url, {
                accessTokenFactory: () => this.token
            })
            .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry intervals
            .build();

        this.setupEventHandlers();
        this.startConnection();
    }

    setupEventHandlers() {
        // Connection events
        this.connection.onclose((error) => {
            console.log('WebSocket connection closed:', error);
            this.handleReconnect();
        });

        this.connection.onreconnecting((error) => {
            console.log('WebSocket reconnecting:', error);
        });

        this.connection.onreconnected((connectionId) => {
            console.log('WebSocket reconnected:', connectionId);
            this.reconnectAttempts = 0;
        });

        // Task events
        this.connection.on('TaskCreated', (task) => {
            console.log('Task created:', task);
            this.handleTaskCreated(task);
        });

        this.connection.on('TaskUpdated', (task) => {
            console.log('Task updated:', task);
            this.handleTaskUpdated(task);
        });

        this.connection.on('TaskStatusChanged', (task) => {
            console.log('Task status changed:', task);
            this.handleTaskStatusChanged(task);
        });

        this.connection.on('TaskAssigned', (task) => {
            console.log('Task assigned:', task);
            this.handleTaskAssigned(task);
        });

        this.connection.on('Error', (error) => {
            console.error('WebSocket error:', error);
            this.handleError(error);
        });
    }

    async startConnection() {
        try {
            await this.connection.start();
            console.log('WebSocket connected successfully');
            this.reconnectAttempts = 0;
        } catch (error) {
            console.error('WebSocket connection failed:', error);
            this.handleReconnect();
        }
    }

    async handleReconnect() {
        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            this.reconnectAttempts++;
            console.log(`Reconnecting... Attempt ${this.reconnectAttempts}`);
            
            setTimeout(() => {
                this.startConnection();
            }, Math.pow(2, this.reconnectAttempts) * 1000); // Exponential backoff
        } else {
            console.error('Max reconnection attempts reached');
            this.handleConnectionFailure();
        }
    }

    async joinProject(projectId) {
        try {
            await this.connection.invoke('JoinProject', projectId);
        } catch (error) {
            console.error('Failed to join project:', error);
        }
    }

    async leaveProject(projectId) {
        try {
            await this.connection.invoke('LeaveProject', projectId);
        } catch (error) {
            console.error('Failed to leave project:', error);
        }
    }

    async updateTaskStatus(taskId, newStatus) {
        try {
            await this.connection.invoke('UpdateTaskStatus', taskId, newStatus);
        } catch (error) {
            console.error('Failed to update task status:', error);
        }
    }

    // Event handlers
    handleTaskCreated(task) {
        // Update UI to show new task
        this.updateTaskList(task);
    }

    handleTaskUpdated(task) {
        // Update UI to reflect task changes
        this.updateTaskInList(task);
    }

    handleTaskStatusChanged(task) {
        // Update UI to show status change
        this.updateTaskStatus(task);
    }

    handleTaskAssigned(task) {
        // Update UI to show assignment change
        this.updateTaskAssignment(task);
    }

    handleError(error) {
        // Show error message to user
        this.showErrorMessage(error);
    }

    handleConnectionFailure() {
        // Show connection failure message
        this.showConnectionFailureMessage();
    }

    // UI update methods (implement based on your UI framework)
    updateTaskList(task) {
        // Implementation depends on your UI framework
        console.log('Updating task list with new task:', task);
    }

    updateTaskInList(task) {
        // Implementation depends on your UI framework
        console.log('Updating task in list:', task);
    }

    updateTaskStatus(task) {
        // Implementation depends on your UI framework
        console.log('Updating task status:', task);
    }

    updateTaskAssignment(task) {
        // Implementation depends on your UI framework
        console.log('Updating task assignment:', task);
    }

    showErrorMessage(error) {
        // Implementation depends on your UI framework
        console.error('WebSocket error:', error);
    }

    showConnectionFailureMessage() {
        // Implementation depends on your UI framework
        console.error('WebSocket connection failed');
    }

    disconnect() {
        if (this.connection) {
            this.connection.stop();
        }
    }
}

// Usage example
const token = 'your-jwt-token';
const webSocket = new SecureTaskWebSocket(token);
webSocket.connect();

// Join a project
webSocket.joinProject('123');

// Update task status
webSocket.updateTaskStatus('456', 'InProgress');
```

## 🧪 Testing WebSocket Security

### 7.1 Security Testing with Postman

```javascript
// Postman WebSocket test script
const token = pm.environment.get('jwt_token');

// Test unauthorized connection
pm.test("Unauthorized connection should be rejected", function () {
    // Try to connect without token
    // Should be rejected
});

// Test with invalid token
pm.test("Invalid token should be rejected", function () {
    // Try to connect with invalid token
    // Should be rejected
});

// Test with valid token
pm.test("Valid token should allow connection", function () {
    // Connect with valid token
    // Should succeed
});

// Test authorization
pm.test("User can only access authorized projects", function () {
    // Try to join unauthorized project
    // Should be rejected
});
```

### 7.2 Load Testing

```bash
# Test WebSocket rate limiting
for i in {1..15}; do
  curl -i -N -H "Connection: Upgrade" \
    -H "Upgrade: websocket" \
    -H "Sec-WebSocket-Version: 13" \
    -H "Sec-WebSocket-Key: SGVsbG8sIHdvcmxkIQ==" \
    "wss://localhost:7001/hubs/securetaskhub?access_token=YOUR_TOKEN"
done
```

## 🔍 Security Best Practices

### 1. Authentication
- **Always authenticate WebSocket connections**
- **Use JWT tokens in headers, not URLs**
- **Validate tokens on every connection**
- **Implement token refresh for long connections**

### 2. Authorization
- **Check user permissions for each operation**
- **Validate resource ownership**
- **Use groups for efficient broadcasting**
- **Log authorization failures**

### 3. Message Validation
- **Validate all incoming messages**
- **Sanitize message content**
- **Limit message size**
- **Rate limit message frequency**

### 4. Connection Management
- **Limit concurrent connections per user**
- **Implement connection timeouts**
- **Monitor connection patterns**
- **Log connection events**

### 5. Error Handling
- **Don't expose sensitive information in errors**
- **Log security events**
- **Implement graceful degradation**
- **Handle connection failures**

## 📋 Security Checklist

- [ ] JWT authentication implemented
- [ ] Authorization checks added
- [ ] Message validation implemented
- [ ] Rate limiting configured
- [ ] Error handling secure
- [ ] Logging implemented
- [ ] Connection monitoring active
- [ ] Security testing completed
- [ ] Documentation updated

## 🚀 Production Considerations

### 1. Scalability
- **Use Redis backplane for multiple instances**
- **Implement connection pooling**
- **Monitor memory usage**
- **Set appropriate timeouts**

### 2. Monitoring
- **Monitor connection counts**
- **Track message rates**
- **Alert on unusual patterns**
- **Log security events**

### 3. Security Headers
- **Set appropriate WebSocket headers**
- **Use secure WebSocket (WSS)**
- **Implement CORS for WebSocket**
- **Add security headers**

---

## 📚 Additional Resources

- [SignalR Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/security)
- [WebSocket Security Best Practices](https://www.owasp.org/index.php/WebSocket_Security_Cheat_Sheet)
- [JWT Authentication for WebSockets](https://auth0.com/blog/websocket-security-considerations/)
- [Real-time Application Security](https://www.owasp.org/index.php/Real-time_Application_Security) 