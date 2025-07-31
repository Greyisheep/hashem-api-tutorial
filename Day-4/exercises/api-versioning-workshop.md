# API Versioning Workshop

## 🎯 Workshop Objectives

**Master API versioning strategies and implement backward compatibility to handle breaking changes gracefully.**

### 📋 What You'll Learn
- Different API versioning strategies
- Backward compatibility techniques
- Migration planning and execution
- Version-specific documentation
- Testing strategies for multiple versions

## 🚀 Workshop Structure

### Phase 1: Versioning Strategies (45 minutes)

#### Step 1: URL Versioning
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersControllerV1 : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // V1 implementation
        return Ok(new { users = new[] { new { id = 1, name = "John" } } });
    }
}

[ApiController]
[Route("api/v2/[controller]")]
public class UsersControllerV2 : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // V2 implementation with enhanced response
        return Ok(new { 
            users = new[] { new { id = 1, name = "John", email = "john@example.com" } },
            pagination = new { page = 1, total = 1 }
        });
    }
}
```

#### Step 2: Header Versioning
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromHeader(Name = "API-Version")] string version = "1")
    {
        switch (version)
        {
            case "1":
                return Ok(new { users = new[] { new { id = 1, name = "John" } } });
            case "2":
                return Ok(new { 
                    users = new[] { new { id = 1, name = "John", email = "john@example.com" } },
                    pagination = new { page = 1, total = 1 }
                });
            default:
                return BadRequest("Unsupported API version");
        }
    }
}
```

#### Step 3: Media Type Versioning
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var acceptHeader = Request.Headers["Accept"].ToString();
        
        if (acceptHeader.Contains("application/vnd.company.v1+json"))
        {
            return Ok(new { users = new[] { new { id = 1, name = "John" } } });
        }
        else if (acceptHeader.Contains("application/vnd.company.v2+json"))
        {
            return Ok(new { 
                users = new[] { new { id = 1, name = "John", email = "john@example.com" } },
                pagination = new { page = 1, total = 1 }
            });
        }
        
        return BadRequest("Unsupported media type");
    }
}
```

### Phase 2: Backward Compatibility (45 minutes)

#### Step 1: Additive Changes Only
```csharp
// V1: Original API
public class UserResponseV1
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// V2: Additive changes (backward compatible)
public class UserResponseV2
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; } // New field
    public DateTime CreatedAt { get; set; } // New field
}

// V3: More additive changes
public class UserResponseV3
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PhoneNumber { get; set; } // New field
    public UserStatus Status { get; set; } // New field
}
```

#### Step 2: Deprecation Strategy
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class UsersControllerV1 : ControllerBase
{
    [HttpGet]
    [Obsolete("This endpoint is deprecated. Use /api/v2/users instead.")]
    public async Task<IActionResult> GetUsers()
    {
        Response.Headers.Add("Deprecation", "true");
        Response.Headers.Add("Sunset", "2024-12-31");
        Response.Headers.Add("Link", "</api/v2/users>; rel=\"successor-version\"");
        
        return Ok(new { users = new[] { new { id = 1, name = "John" } } });
    }
}
```

#### Step 3: Feature Flags
```csharp
public class FeatureFlags
{
    public bool EnableV2Features { get; set; }
    public bool EnableV3Features { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly FeatureFlags _featureFlags;
    
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        if (_featureFlags.EnableV3Features)
        {
            return Ok(new UserResponseV3 { /* V3 response */ });
        }
        else if (_featureFlags.EnableV2Features)
        {
            return Ok(new UserResponseV2 { /* V2 response */ });
        }
        else
        {
            return Ok(new UserResponseV1 { /* V1 response */ });
        }
    }
}
```

### Phase 3: Migration Strategy (30 minutes)

#### Step 1: Migration Planning
```markdown
📋 Migration Timeline:
- Phase 1: Deploy V2 alongside V1 (3 months)
- Phase 2: Deprecate V1, encourage V2 adoption (6 months)
- Phase 3: Sunset V1, V2 becomes primary (9 months)
- Phase 4: Introduce V3, deprecate V2 (12 months)

🔄 Migration Steps:
1. Deploy new version alongside existing
2. Update documentation and examples
3. Notify developers of deprecation
4. Provide migration guides
5. Monitor usage and adoption
6. Gradually phase out old version
```

#### Step 2: Migration Tools
```csharp
public class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Detect API version from request
        var version = GetApiVersion(context);
        
        // Route to appropriate controller
        context.Items["ApiVersion"] = version;
        
        await _next(context);
    }
    
    private string GetApiVersion(HttpContext context)
    {
        // Check URL path
        if (context.Request.Path.Value?.Contains("/v2/") == true)
            return "2";
        if (context.Request.Path.Value?.Contains("/v1/") == true)
            return "1";
            
        // Check header
        if (context.Request.Headers.TryGetValue("API-Version", out var headerVersion))
            return headerVersion;
            
        // Default to V1
        return "1";
    }
}
```

### Phase 4: Testing Strategy (30 minutes)

#### Step 1: Version-Specific Tests
```csharp
[TestFixture]
public class UsersApiVersionTests
{
    [Test]
    public async Task GetUsers_V1_ReturnsV1Response()
    {
        // Arrange
        var client = CreateTestClient();
        
        // Act
        var response = await client.GetAsync("/api/v1/users");
        
        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(content);
        
        // V1 should not have email field
        Assert.That(result.users[0].email, Is.Null);
    }
    
    [Test]
    public async Task GetUsers_V2_ReturnsV2Response()
    {
        // Arrange
        var client = CreateTestClient();
        
        // Act
        var response = await client.GetAsync("/api/v2/users");
        
        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<dynamic>(content);
        
        // V2 should have email field
        Assert.That(result.users[0].email, Is.Not.Null);
        Assert.That(result.pagination, Is.Not.Null);
    }
}
```

#### Step 2: Backward Compatibility Tests
```csharp
[Test]
public async Task GetUsers_V1Deprecated_ReturnsDeprecationHeaders()
{
    // Arrange
    var client = CreateTestClient();
    
    // Act
    var response = await client.GetAsync("/api/v1/users");
    
    // Assert
    Assert.That(response.Headers.Contains("Deprecation"), Is.True);
    Assert.That(response.Headers.Contains("Sunset"), Is.True);
    Assert.That(response.Headers.Contains("Link"), Is.True);
}
```

## 🛠️ Hands-On Exercise

### Exercise 1: Implement Versioning for Squad.API
```csharp
// Add versioning to the Squad.API payment endpoints
// Implement both V1 and V2 versions
// Ensure backward compatibility
```

**Your Task**:
1. Create V1 payment endpoints (existing implementation)
2. Create V2 payment endpoints with enhanced features
3. Implement deprecation strategy for V1
4. Add comprehensive tests for both versions
5. Create migration documentation

### Exercise 2: Version Migration Planning
```markdown
📋 Plan the migration from V1 to V2 for Squad.API:

1. **Current State Analysis**:
   - List all V1 endpoints
   - Identify breaking changes needed
   - Document current usage patterns

2. **Migration Strategy**:
   - Timeline for V2 deployment
   - Deprecation schedule for V1
   - Communication plan for developers

3. **Testing Strategy**:
   - Version-specific test suites
   - Backward compatibility tests
   - Integration tests for migration

4. **Documentation Updates**:
   - API documentation for both versions
   - Migration guides
   - Code examples for each version
```

## 📊 Assessment Criteria

### Versioning Implementation (40%)
- ✅ Multiple versioning strategies implemented
- ✅ Backward compatibility maintained
- ✅ Deprecation strategy in place
- ✅ Clear version routing

### Testing Coverage (30%)
- ✅ Version-specific tests
- ✅ Backward compatibility tests
- ✅ Migration tests
- ✅ Integration tests

### Documentation (20%)
- ✅ API documentation for each version
- ✅ Migration guides
- ✅ Code examples
- ✅ Deprecation notices

### Migration Planning (10%)
- ✅ Clear migration timeline
- ✅ Communication strategy
- ✅ Rollback plan
- ✅ Monitoring strategy

## 🎯 Success Metrics

### By End of Workshop, You Should:
1. ✅ Implement multiple versioning strategies
2. ✅ Maintain backward compatibility
3. ✅ Create comprehensive test suites
4. ✅ Document migration strategy
5. ✅ Plan and execute version migration

## 📚 Additional Resources

### Tools & Libraries
- **ASP.NET Core Versioning** - Official versioning library
- **Swagger/OpenAPI** - API documentation with versioning
- **Postman** - API testing with version management
- **xUnit** - Testing framework for version-specific tests

### Documentation
- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [REST API Versioning Best Practices](https://restfulapi.net/versioning/)
- [API Evolution](https://apisyouwonthate.com/blog/api-evolution-for-rest-http-apis)

### Best Practices
- Always maintain backward compatibility
- Use additive changes when possible
- Provide clear migration paths
- Document breaking changes thoroughly
- Test all versions thoroughly
- Monitor version usage and adoption

---

**Remember**: API versioning is about managing change gracefully. Plan carefully, communicate clearly, and always provide a path forward for your API consumers! 