# OWASP API Security Top 10 Workshop

## 🎯 Workshop Objectives
By the end of this workshop, students will be able to:
- [ ] Identify the OWASP API Security Top 10 vulnerabilities
- [ ] Understand real-world attack scenarios
- [ ] Implement proper mitigation strategies
- [ ] Apply security-first thinking to API design

## 🚨 OWASP API Security Top 10 (2023)

### API1:2023 - Broken Object Level Authorization (BOLA)

#### 🎯 What is it?
When an API fails to properly validate that a user has permission to access a specific object.

#### 🧪 Vulnerable Code Example
```csharp
// VULNERABLE: No authorization check
[HttpGet("tasks/{taskId}")]
public async Task<IActionResult> GetTask(int taskId)
{
    var task = await _taskRepository.GetByIdAsync(taskId);
    return Ok(task); // Anyone can access any task!
}
```

#### ✅ Secure Implementation
```csharp
// SECURE: Check user ownership
[HttpGet("tasks/{taskId}")]
[Authorize]
public async Task<IActionResult> GetTask(int taskId)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var task = await _taskRepository.GetByIdAsync(taskId);
    
    if (task == null || task.AssignedUserId.ToString() != userId)
    {
        return Forbid();
    }
    
    return Ok(task);
}
```

#### 🎭 Attack Scenario
1. Attacker logs in with account A
2. Attacker tries to access task 123 (belongs to user B)
3. API returns task data without checking ownership
4. Attacker can now access any task in the system

---

### API2:2023 - Broken Authentication

#### 🎯 What is it?
Weak authentication mechanisms that can be bypassed or exploited.

#### 🧪 Vulnerable Code Example
```csharp
// VULNERABLE: Weak password requirements
public class User
{
    public string Password { get; set; } = string.Empty; // Plain text!
}

// VULNERABLE: No rate limiting on login
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    // No rate limiting - brute force possible
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
    {
        return Ok("Logged in");
    }
    return Unauthorized();
}
```

#### ✅ Secure Implementation
```csharp
// SECURE: Strong password hashing and rate limiting
[HttpPost("login")]
[RateLimit(5, 5)] // 5 attempts per 5 minutes
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
    {
        // Use strong JWT with short expiration
        var token = GenerateJwtToken(user, TimeSpan.FromMinutes(15));
        return Ok(new { token });
    }
    
    // Log failed attempts
    _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
    return Unauthorized();
}
```

---

### API3:2023 - Broken Object Property Level Authorization

#### 🎯 What is it?
When an API allows users to modify object properties they shouldn't have access to.

#### 🧪 Vulnerable Code Example
```csharp
// VULNERABLE: Mass assignment vulnerability
[HttpPut("users/{userId}")]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto dto)
{
    var user = await _userRepository.GetByIdAsync(userId);
    
    // DANGEROUS: Updates all properties including sensitive ones
    user.Email = dto.Email;
    user.Role = dto.Role; // Attacker can make themselves admin!
    user.IsActive = dto.IsActive;
    
    await _userRepository.UpdateAsync(user);
    return Ok(user);
}
```

#### ✅ Secure Implementation
```csharp
// SECURE: Explicit property mapping
[HttpPut("users/{userId}")]
[Authorize]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto dto)
{
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId.ToString() != currentUserId)
    {
        return Forbid();
    }
    
    var user = await _userRepository.GetByIdAsync(userId);
    
    // Only allow updating safe properties
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.PhoneNumber = dto.PhoneNumber;
    // Role and IsActive are NOT updated!
    
    await _userRepository.UpdateAsync(user);
    return Ok(user);
}
```

---

### API4:2023 - Unrestricted Resource Consumption

#### 🎯 What is it?
APIs that don't limit resource usage, leading to DoS attacks or excessive costs.

#### 🧪 Vulnerable Code Example
```csharp
// VULNERABLE: No limits on data retrieval
[HttpGet("tasks")]
public async Task<IActionResult> GetTasks()
{
    // DANGEROUS: No pagination, no limits
    var tasks = await _taskRepository.GetAllAsync();
    return Ok(tasks); // Could return millions of records!
}

// VULNERABLE: No file size limits
[HttpPost("upload")]
public async Task<IActionResult> UploadFile(IFormFile file)
{
    // No size limit - attacker can upload huge files
    var path = Path.Combine(_uploadPath, file.FileName);
    using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);
    return Ok("Uploaded");
}
```

#### ✅ Secure Implementation
```csharp
// SECURE: Pagination and limits
[HttpGet("tasks")]
public async Task<IActionResult> GetTasks([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    // Enforce reasonable limits
    pageSize = Math.Min(pageSize, 100); // Max 100 items per page
    page = Math.Max(page, 1);
    
    var tasks = await _taskRepository.GetPagedAsync(page, pageSize);
    return Ok(new { tasks, page, pageSize, totalCount = tasks.Count });
}

// SECURE: File size limits
[HttpPost("upload")]
public async Task<IActionResult> UploadFile(IFormFile file)
{
    const int maxFileSize = 10 * 1024 * 1024; // 10MB limit
    
    if (file.Length > maxFileSize)
    {
        return BadRequest("File too large");
    }
    
    // Validate file type
    var allowedTypes = new[] { ".jpg", ".png", ".pdf" };
    var extension = Path.GetExtension(file.FileName).ToLower();
    if (!allowedTypes.Contains(extension))
    {
        return BadRequest("Invalid file type");
    }
    
    var path = Path.Combine(_uploadPath, file.FileName);
    using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);
    return Ok("Uploaded");
}
```

---

### API5:2023 - Broken Function Level Authorization

#### 🎯 What is it?
When an API exposes administrative functions to regular users.

#### 🧪 Vulnerable Code Example
```csharp
// VULNERABLE: Admin function exposed to all users
[HttpDelete("users/{userId}")]
public async Task<IActionResult> DeleteUser(int userId)
{
    // No role check - any user can delete any user!
    await _userRepository.DeleteAsync(userId);
    return Ok("User deleted");
}

// VULNERABLE: Sensitive operation without proper authorization
[HttpPost("system/backup")]
public async Task<IActionResult> CreateBackup()
{
    // No admin check - any user can trigger backup!
    await _backupService.CreateBackupAsync();
    return Ok("Backup created");
}
```

#### ✅ Secure Implementation
```csharp
// SECURE: Role-based authorization
[HttpDelete("users/{userId}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser(int userId)
{
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser.Id == userId)
    {
        return BadRequest("Cannot delete yourself");
    }
    
    await _userRepository.DeleteAsync(userId);
    return Ok("User deleted");
}

// SECURE: Multiple authorization checks
[HttpPost("system/backup")]
[Authorize(Roles = "Admin")]
[RequirePermission("System.Backup")]
public async Task<IActionResult> CreateBackup()
{
    // Additional audit logging
    _logger.LogInformation("Backup initiated by user: {UserId}", 
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    
    await _backupService.CreateBackupAsync();
    return Ok("Backup created");
}
```

---

## 🎭 Interactive Exercise: Vulnerability Hunt

### Exercise 1: Find the BOLA Vulnerability

**Scenario**: You're reviewing the TaskFlow API code. Look at this endpoint:

```csharp
[HttpGet("projects/{projectId}/tasks")]
public async Task<IActionResult> GetProjectTasks(int projectId)
{
    var tasks = await _taskRepository.GetByProjectIdAsync(projectId);
    return Ok(tasks);
}
```

**Questions**:
1. What's the vulnerability here?
2. How could an attacker exploit it?
3. How would you fix it?

**Answer**:
<details>
<summary>Click to reveal</summary>

**Vulnerability**: Broken Object Level Authorization (BOLA)

**Exploitation**:
1. Attacker logs in with account A
2. Attacker knows project 123 belongs to user B
3. Attacker calls `GET /projects/123/tasks`
4. API returns all tasks from project 123 without checking if attacker owns the project

**Fix**:
```csharp
[HttpGet("projects/{projectId}/tasks")]
[Authorize]
public async Task<IActionResult> GetProjectTasks(int projectId)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var project = await _projectRepository.GetByIdAsync(projectId);
    
    if (project == null || project.OwnerId.ToString() != userId)
    {
        return Forbid();
    }
    
    var tasks = await _taskRepository.GetByProjectIdAsync(projectId);
    return Ok(tasks);
}
```
</details>

---

### Exercise 2: Mass Assignment Attack

**Scenario**: Review this user update endpoint:

```csharp
[HttpPut("users/{userId}")]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto dto)
{
    var user = await _userRepository.GetByIdAsync(userId);
    
    // Update all properties from DTO
    user.Email = dto.Email;
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.Role = dto.Role; // DANGER!
    user.IsActive = dto.IsActive; // DANGER!
    
    await _userRepository.UpdateAsync(user);
    return Ok(user);
}
```

**Questions**:
1. What's the vulnerability?
2. What could an attacker do?
3. How would you fix it?

**Answer**:
<details>
<summary>Click to reveal</summary>

**Vulnerability**: Mass Assignment / Broken Object Property Level Authorization

**Exploitation**:
1. Attacker sends request with `"role": "Admin"`
2. API updates the user's role to Admin
3. Attacker now has administrative privileges

**Fix**:
```csharp
[HttpPut("users/{userId}")]
[Authorize]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto dto)
{
    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId.ToString() != currentUserId)
    {
        return Forbid();
    }
    
    var user = await _userRepository.GetByIdAsync(userId);
    
    // Only allow updating safe properties
    user.FirstName = dto.FirstName;
    user.LastName = dto.LastName;
    user.Email = dto.Email;
    // Role and IsActive are NOT updated!
    
    await _userRepository.UpdateAsync(user);
    return Ok(user);
}
```
</details>

---

## 🛡️ Security Best Practices Checklist

### Authentication & Authorization
- [ ] Use strong password hashing (bcrypt, Argon2)
- [ ] Implement rate limiting on login endpoints
- [ ] Use short-lived JWT tokens with refresh tokens
- [ ] Validate user ownership for all resource access
- [ ] Implement role-based access control (RBAC)
- [ ] Use principle of least privilege

### Input Validation
- [ ] Validate all user inputs
- [ ] Use parameterized queries (prevent SQL injection)
- [ ] Sanitize output (prevent XSS)
- [ ] Validate file uploads (type, size, content)
- [ ] Use strong typing and validation libraries

### Resource Management
- [ ] Implement pagination for large datasets
- [ ] Set file upload size limits
- [ ] Use rate limiting on all endpoints
- [ ] Monitor resource usage
- [ ] Implement timeouts for long-running operations

### Security Headers
- [ ] Set Content-Security-Policy
- [ ] Use HTTPS only (HSTS)
- [ ] Set X-Frame-Options to prevent clickjacking
- [ ] Set X-Content-Type-Options to prevent MIME sniffing
- [ ] Configure CORS properly

### Logging & Monitoring
- [ ] Log authentication attempts (success/failure)
- [ ] Log authorization failures
- [ ] Monitor for unusual patterns
- [ ] Use structured logging
- [ ] Don't log sensitive data

---

## 🎯 Real-World Attack Scenarios

### Scenario 1: E-commerce API
**Vulnerability**: BOLA in order endpoint
**Attack**: Attacker changes order ID to access other users' orders
**Impact**: Data breach, privacy violation

### Scenario 2: Banking API
**Vulnerability**: Mass assignment in account update
**Attack**: Attacker sets account balance to arbitrary value
**Impact**: Financial fraud, system compromise

### Scenario 3: Social Media API
**Vulnerability**: Unrestricted resource consumption
**Attack**: Attacker requests all posts without pagination
**Impact**: DoS attack, high server costs

---

## 🧪 Hands-On Exercise

### Task: Secure the TaskFlow API

1. **Review the current TaskFlow API code**
2. **Identify potential OWASP Top 10 vulnerabilities**
3. **Implement fixes for each vulnerability**
4. **Test your security improvements**

### Vulnerable Endpoints to Fix:

```csharp
// 1. Fix this BOLA vulnerability
[HttpGet("tasks/{taskId}")]
public async Task<IActionResult> GetTask(int taskId)
{
    var task = await _taskRepository.GetByIdAsync(taskId);
    return Ok(task);
}

// 2. Fix this mass assignment vulnerability
[HttpPut("users/{userId}")]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateDto dto)
{
    var user = await _userRepository.GetByIdAsync(userId);
    // Update all properties from DTO
    await _userRepository.UpdateAsync(user);
    return Ok(user);
}

// 3. Fix this broken authentication
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    // No rate limiting, weak password validation
    var user = await _userRepository.GetByEmailAsync(dto.Email);
    if (user.Password == dto.Password) // Plain text!
    {
        return Ok("Logged in");
    }
    return Unauthorized();
}
```

### Your Task:
1. Identify the vulnerabilities in each endpoint
2. Implement secure versions
3. Add proper authorization checks
4. Add input validation
5. Add rate limiting where appropriate
6. Test your implementations

---

## 📊 Assessment Questions

### Knowledge Check
1. **What is BOLA and how do you prevent it?**
2. **Why is mass assignment dangerous?**
3. **What are the risks of unrestricted resource consumption?**
4. **How do you implement proper authentication?**
5. **What security headers should you use?**

### Practical Application
1. **Can you identify vulnerabilities in code?**
2. **Can you implement secure alternatives?**
3. **Do you understand the business impact of security flaws?**
4. **Can you apply security-first thinking to API design?**

---

## 🎓 Learning Outcomes

By completing this workshop, you should be able to:

- [ ] Identify OWASP API Security Top 10 vulnerabilities
- [ ] Understand real-world attack scenarios
- [ ] Implement proper security controls
- [ ] Apply security-first thinking to API design
- [ ] Use security testing tools effectively
- [ ] Monitor and log security events
- [ ] Respond to security incidents appropriately

---

## 📚 Additional Resources

- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [Security Headers](https://securityheaders.com/)
- [JWT.io](https://jwt.io/) - JWT token debugger 