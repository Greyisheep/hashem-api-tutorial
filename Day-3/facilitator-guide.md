# Day 3 Facilitator Guide - API Security Best Practices

## 🎯 Day 3 Objectives
By 4:00 PM, students should be able to:
- [ ] Implement OAuth 2.0 with Google authentication
- [ ] Apply OWASP API Security Top 10 principles
- [ ] Implement rate limiting and throttling
- [ ] Secure secrets management and environment configuration
- [ ] Add comprehensive security headers and middleware
- [ ] Implement authenticated WebSocket connections
- [ ] Design secure webhook endpoints
- [ ] Apply security-first development practices

**Key Deliverable**: A production-hardened TaskFlow API with OAuth 2.0, rate limiting, security headers, and comprehensive security monitoring.

---

## ⏰ Detailed Schedule & Timing

### 10:00-10:30 AM: Security Knowledge Review & OWASP Discussion (30 min)

#### Pre-flight Checklist:
- [ ] All students have Google Cloud Console access (or demo accounts ready)
- [ ] TaskFlow API running with current security baseline
- [ ] Postman collections updated for security testing
- [ ] OWASP ZAP or similar security testing tool ready
- [ ] Sticky notes for security threat modeling
- [ ] Timer visible to group

#### Opening Script:
"Yesterday we built APIs. Today we secure them. Let's start with what you learned about OWASP and OAuth 2.0. What security concerns keep you up at night?"

#### Activity: Security Knowledge Review (20 min)
**Format**: Interactive discussion with sticky notes
**Materials**: Sticky notes, whiteboard, OWASP reference materials

**Your Role**: Facilitate discussion, assess knowledge gaps, set security mindset

**Discussion Points**:
1. **OWASP API Security Top 10**: What did you learn?
   - Have students share key vulnerabilities they researched
   - Use sticky notes to capture common themes
   - Identify knowledge gaps to address

2. **OAuth 2.0 Understanding**: What flows do you know?
   - Authorization Code Flow vs Client Credentials
   - PKCE importance for SPAs
   - Token types and their purposes

3. **Real-world Security Concerns**: What keeps you up at night?
   - Data breaches, authentication bypass, rate limiting
   - Use this to set the tone for the day

**Key Questions to Ask**:
- "What's the difference between authentication and authorization?"
- "Why is rate limiting important for APIs?"
- "What happens if you don't validate user input?"
- "How do you secure sensitive data in transit and at rest?"

**Expected Answers**:
<details><summary>**Authentication vs Authorization**</summary>
- Authentication: Verifying who you are (login)
- Authorization: Verifying what you can do (permissions)
- Example: Login (auth) vs accessing admin panel (authorization)
</details>

<details><summary>**Rate Limiting Importance**</summary>
- Prevents brute force attacks
- Protects against DoS attacks
- Controls API usage costs
- Ensures fair resource distribution
</details>

<details><summary>**Input Validation Risks**</summary>
- SQL injection attacks
- XSS (Cross-site scripting)
- Command injection
- Path traversal attacks
</details>

<details><summary>**Data Security**</summary>
- HTTPS for transit security
- Encryption at rest (database, files)
- Secure key management
- Regular security audits
</details>

### 10:30-10:45 AM: Break (15 min)
**Your prep time**: Set up Google OAuth demo environment

### 10:45-12:00 PM: Module 1 - OAuth 2.0 Implementation (75 min)

#### OAuth 2.0 Deep Dive (20 min)
**Materials**: [`demos/oauth2-implementation-demo.md`](./demos/oauth2-implementation-demo.md)
**Your Role**: Demonstrate live OAuth flow, explain PKCE importance

**Key Teaching Points**:
1. **OAuth 2.0 Flow Explanation**:
   - Show the sequence diagram
   - Explain each step in the flow
   - Highlight security considerations

2. **PKCE (Proof Key for Code Exchange)**:
   - Why it's needed for SPAs
   - How it prevents code interception
   - Implementation in .NET

3. **Token Security**:
   - Access tokens vs refresh tokens
   - Token storage best practices
   - Token validation and expiration

**Demo Script**:
"Let's see OAuth 2.0 in action. I'll show you how to implement Google OAuth in our TaskFlow API."

1. **Show the OAuth Flow**:
   - Open Google Cloud Console
   - Show OAuth 2.0 configuration
   - Demonstrate the authorization flow
   - Show token exchange process

2. **Explain Security Considerations**:
   - "Notice how we never see the user's password"
   - "The authorization code is exchanged for tokens"
   - "PKCE prevents code interception attacks"

#### Google OAuth Setup Walkthrough (25 min)
**Activity**: Step-by-step Google OAuth configuration
**Your Role**: Guide students through setup, troubleshoot issues

**Setup Steps**:
1. **Google Cloud Console Setup**:
   - Create new project or use existing
   - Enable Google+ API and OAuth2 API
   - Configure OAuth consent screen
   - Create OAuth 2.0 credentials

2. **TaskFlow API Integration**:
   - Install required NuGet packages
   - Configure authentication in Program.cs
   - Update appsettings.json
   - Test the OAuth flow

**Common Issues & Solutions**:
- **Redirect URI mismatch**: Ensure exact match in Google Console
- **CORS issues**: Configure CORS properly for frontend
- **Token validation**: Verify JWT configuration
- **Scope issues**: Ensure required scopes are requested

**Student Activities**:
- Have students follow along with their own Google projects
- Pair students to troubleshoot issues
- Use sticky notes to track common problems

#### OAuth Implementation in TaskFlow (30 min)
**Materials**: [`implementation/oauth2-setup-guide.md`](./implementation/oauth2-setup-guide.md)
**Your Role**: Code-along facilitator, security reviewer

**Implementation Steps**:
1. **Add Google OAuth to TaskFlow**:
   ```csharp
   // Add to Program.cs
   builder.Services.AddAuthentication()
       .AddGoogle(options =>
       {
           options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
           options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
           options.UsePkce = true;
       });
   ```

2. **Create AuthController**:
   - Implement Google login endpoint
   - Handle OAuth callback
   - Generate JWT tokens
   - Validate user permissions

3. **Security Enhancements**:
   - Add PKCE support
   - Implement token validation
   - Add security headers
   - Configure CORS properly

**Code Review Points**:
- "Why do we use PKCE for SPAs?"
- "How do we prevent token theft?"
- "What happens if the OAuth callback is intercepted?"
- "How do we handle token refresh?"

### 12:00-1:00 PM: Lunch (60 min)
**Your prep time**: Prepare rate limiting and security headers implementation

### 1:00-2:15 PM: Module 2 - Rate Limiting & Security Headers (75 min)

#### Rate Limiting Fundamentals (20 min)
**Discussion**: "Why do we need rate limiting?"
**Your Role**: Explain concepts, show real-world examples

**Key Concepts**:
1. **Rate Limiting Strategies**:
   - Fixed window: 100 requests per minute
   - Sliding window: More accurate, fairer
   - Token bucket: Smooths out bursts
   - Leaky bucket: Controls flow rate

2. **Implementation Locations**:
   - API Gateway level (NGINX, Azure API Management)
   - Application level (middleware)
   - Database level (query limits)

3. **Rate Limiting Headers**:
   ```
   X-RateLimit-Limit: 100
   X-RateLimit-Remaining: 95
   X-RateLimit-Reset: Wed, 21 Oct 2023 07:28:00 GMT
   Retry-After: 60
   ```

**Real-world Examples**:
- **Twitter API**: 300 requests per 15 minutes
- **GitHub API**: 5000 requests per hour for authenticated users
- **Stripe API**: 100 requests per second

#### Rate Limiting Implementation (30 min)
**Materials**: [`implementation/rate-limiting-guide.md`](./implementation/rate-limiting-guide.md)
**Your Role**: Code-along facilitator, explain implementation choices

**Implementation Steps**:
1. **Install AspNetCoreRateLimit**:
   ```bash
   dotnet add package AspNetCoreRateLimit
   ```

2. **Configure Rate Limiting**:
   ```csharp
   builder.Services.Configure<IpRateLimitOptions>(options =>
   {
       options.GeneralRules = new List<RateLimitRule>
       {
           new RateLimitRule
           {
               Endpoint = "*",
               Limit = 100,
               Period = "1m"
           }
       };
   });
   ```

3. **Add Custom Rate Limiting**:
   - Per-user rate limiting
   - Per-endpoint rate limiting
   - API key-based rate limiting

**Student Activities**:
- Have students implement rate limiting on their endpoints
- Test rate limiting with Postman
- Monitor rate limit headers
- Discuss appropriate limits for different endpoints

#### Security Headers & Middleware (25 min)
**Materials**: [`implementation/security-headers-guide.md`](./implementation/security-headers-guide.md)
**Your Role**: Explain security headers, demonstrate implementation

**Security Headers to Implement**:
1. **Strict-Transport-Security**: Forces HTTPS
2. **X-Frame-Options**: Prevents clickjacking
3. **X-Content-Type-Options**: Prevents MIME sniffing
4. **X-XSS-Protection**: XSS protection for older browsers
5. **Content-Security-Policy**: Controls resource loading
6. **Referrer-Policy**: Controls referrer information
7. **Permissions-Policy**: Controls feature access

**Implementation**:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});
```

**Testing Security Headers**:
- Use browser developer tools
- Use curl to check headers
- Use online security header checkers
- Verify headers in production

### 2:15-2:30 PM: Break (15 min)
**Your prep time**: Prepare WebSocket and webhook security demos

### 2:30-3:45 PM: Module 3 - Advanced Security Patterns (75 min)

#### Authenticated WebSockets (25 min)
**Materials**: [`demos/websocket-security-demo.md`](./demos/websocket-security-demo.md)
**Your Role**: Demonstrate WebSocket security, explain authentication flow

**Key Teaching Points**:
1. **WebSocket Security Challenges**:
   - No built-in authentication
   - Token exposure in URLs
   - No authorization checks
   - Message validation needed

2. **Secure WebSocket Implementation**:
   - JWT authentication in headers
   - Authorization checks per operation
   - Message validation and sanitization
   - Rate limiting for connections

3. **Real-time Security Considerations**:
   - Group-based broadcasting
   - User-specific notifications
   - Connection monitoring
   - Error handling

**Demo Script**:
"Let's implement secure WebSocket connections for real-time task updates."

1. **Show Vulnerable WebSocket**:
   - No authentication
   - Anyone can connect
   - No authorization checks

2. **Implement Secure WebSocket**:
   - Add JWT authentication
   - Implement authorization checks
   - Add message validation
   - Configure rate limiting

**Student Activities**:
- Implement WebSocket authentication
- Test secure connections
- Monitor connection patterns
- Handle connection failures

#### Secure Webhook Implementation (25 min)
**Materials**: [`implementation/webhook-security-guide.md`](./implementation/webhook-security-guide.md)
**Your Role**: Explain webhook security, demonstrate signature validation

**Webhook Security Concepts**:
1. **Webhook Vulnerabilities**:
   - Replay attacks
   - Man-in-the-middle attacks
   - Unauthorized webhook calls
   - Data tampering

2. **Security Measures**:
   - HMAC signature validation
   - Timestamp validation
   - Idempotency keys
   - Rate limiting

**Implementation**:
```csharp
[HttpPost("webhooks/task-updated")]
public async Task<IActionResult> TaskUpdatedWebhook([FromBody] WebhookPayload payload, [FromHeader] string signature)
{
    // Validate signature
    var expectedSignature = GenerateHmacSignature(payload, _webhookSecret);
    if (signature != expectedSignature)
    {
        return Unauthorized();
    }
    
    // Validate timestamp (prevent replay attacks)
    if (DateTime.UtcNow.Subtract(payload.Timestamp).TotalMinutes > 5)
    {
        return BadRequest("Webhook too old");
    }
    
    // Process webhook
    await ProcessTaskUpdate(payload);
    return Ok();
}
```

#### Secrets Management (25 min)
**Materials**: [`implementation/secrets-management-guide.md`](./implementation/secrets-management-guide.md)
**Your Role**: Explain secrets management, demonstrate Azure Key Vault

**Secrets Management Concepts**:
1. **Why Secrets Management Matters**:
   - Never commit secrets to source code
   - Rotate secrets regularly
   - Use secure storage
   - Audit secret access

2. **Azure Key Vault Integration**:
   - Store secrets securely
   - Access secrets programmatically
   - Monitor secret usage
   - Implement secret rotation

**Implementation**:
```csharp
// Add Azure Key Vault
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());

// Access secrets
var secretValue = configuration["MySecretKey"];
```

### 3:45-4:00 PM: Break (15 min)
**Your prep time**: Prepare security testing and final exercises

### 4:00-5:00 PM: Module 4 - Security Testing & OWASP Top 10 (60 min)

#### OWASP API Security Top 10 Review (20 min)
**Materials**: [`exercises/owasp-top10-workshop.md`](./exercises/owasp-top10-workshop.md)
**Your Role**: Facilitate interactive workshop, guide vulnerability identification

**Workshop Structure**:
1. **Vulnerability Hunt** (10 min):
   - Show vulnerable code examples
   - Have students identify issues
   - Discuss potential attacks
   - Brainstorm fixes

2. **Secure Implementation** (10 min):
   - Show secure code examples
   - Explain security measures
   - Discuss trade-offs
   - Review best practices

**Interactive Activities**:
- **Code Review**: Students review vulnerable code
- **Attack Simulation**: Students try to exploit vulnerabilities
- **Fix Implementation**: Students implement secure alternatives
- **Security Discussion**: Group discussion of security implications

#### Security Testing Workshop (25 min)
**Activity**: Hands-on security testing
**Your Role**: Guide testing process, explain tools, interpret results

**Testing Tools**:
1. **OWASP ZAP**: Automated security testing
2. **Postman**: Manual security testing
3. **curl**: Command-line testing
4. **Browser DevTools**: Header inspection

**Testing Scenarios**:
1. **Authentication Testing**:
   - Test with invalid tokens
   - Test with expired tokens
   - Test with missing tokens
   - Test with malformed tokens

2. **Authorization Testing**:
   - Test access to unauthorized resources
   - Test privilege escalation
   - Test role-based access
   - Test resource ownership

3. **Input Validation Testing**:
   - Test SQL injection
   - Test XSS attacks
   - Test command injection
   - Test path traversal

4. **Rate Limiting Testing**:
   - Test rate limit enforcement
   - Test rate limit headers
   - Test rate limit bypass attempts
   - Test rate limit recovery

**Student Activities**:
- Run OWASP ZAP against TaskFlow API
- Test authentication endpoints
- Verify security headers
- Document security findings

#### Production Security Checklist (15 min)
**Materials**: [`exercises/production-security-checklist.md`](./exercises/production-security-checklist.md)
**Your Role**: Review checklist, ensure comprehensive coverage

**Checklist Review**:
1. **Authentication & Authorization**:
   - OAuth 2.0 properly configured
   - JWT tokens secure
   - User management secure
   - Authorization checks implemented

2. **Input Validation & Sanitization**:
   - All inputs validated
   - SQL injection prevented
   - XSS prevented
   - File uploads secure

3. **Rate Limiting & Throttling**:
   - Rate limiting configured
   - Appropriate limits set
   - Monitoring implemented
   - Headers returned

4. **Security Headers & CORS**:
   - Security headers configured
   - CORS properly configured
   - HTTPS enforced
   - Headers validated

5. **Logging & Monitoring**:
   - Security events logged
   - Monitoring active
   - Alerts configured
   - Metrics tracked

**Student Activities**:
- Complete security checklist
- Identify missing security measures
- Prioritize security improvements
- Create security action plan

---

## 🎯 Key Learning Outcomes

### OAuth 2.0 & Authentication
- Understand OAuth 2.0 flows (Authorization Code, PKCE)
- Implement Google OAuth in .NET applications
- Secure token storage and refresh mechanisms
- Handle authentication in SPAs and mobile apps

### Rate Limiting & Throttling
- Implement rate limiting strategies
- Configure per-user and per-IP limits
- Monitor and log rate limit violations
- Handle rate limit responses gracefully

### Security Headers & Middleware
- Implement comprehensive security headers
- Configure CORS properly
- Set up Content Security Policy
- Add security middleware to .NET applications

### Advanced Security Patterns
- Secure WebSocket connections with JWT
- Implement webhook signature validation
- Use Azure Key Vault for secrets management
- Apply security-first development practices

### OWASP API Security Top 10
- Identify and prevent common API vulnerabilities
- Implement proper authorization checks
- Secure data exposure and input validation
- Monitor and log security events

---

## 🛠️ Technical Stack

### Authentication & Authorization
- **OAuth 2.0**: Google OAuth implementation
- **JWT**: Token-based authentication
- **.NET Identity**: User management and authentication
- **PKCE**: Proof Key for Code Exchange for SPAs

### Security Middleware
- **Rate Limiting**: AspNetCoreRateLimit package
- **Security Headers**: Custom middleware implementation
- **CORS**: Proper cross-origin configuration
- **CSP**: Content Security Policy headers

### Secrets Management
- **Azure Key Vault**: Secure secrets storage
- **Environment Variables**: Local development security
- **Configuration**: Secure appsettings management

### Security Testing
- **OWASP ZAP**: Automated security testing
- **Postman**: Manual security testing
- **Security Headers**: Validation tools
- **JWT Debugger**: Token inspection

---

## 📚 Resources & References

### Documentation
- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [.NET Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/)

### Tools
- [Google Cloud Console](https://console.cloud.google.com/)
- [OWASP ZAP](https://owasp.org/www-project-zap/)
- [JWT.io](https://jwt.io/)
- [Security Headers](https://securityheaders.com/)

### Best Practices
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [Azure Security Best Practices](https://docs.microsoft.com/en-us/azure/security/)

---

## 🎓 Assessment & Evaluation

### Knowledge Check
- [ ] Can implement OAuth 2.0 with PKCE
- [ ] Understands rate limiting strategies
- [ ] Can configure security headers
- [ ] Knows OWASP API Security Top 10
- [ ] Can implement secure WebSocket connections
- [ ] Understands secrets management best practices

### Practical Skills
- [ ] Successfully integrated Google OAuth
- [ ] Implemented rate limiting in TaskFlow API
- [ ] Added comprehensive security headers
- [ ] Created secure webhook endpoints
- [ ] Performed security testing with OWASP ZAP
- [ ] Applied production security checklist

### Security Mindset
- [ ] Thinks security-first in API design
- [ ] Understands threat modeling
- [ ] Can identify common vulnerabilities
- [ ] Knows how to secure sensitive data
- [ ] Understands defense-in-depth principles

---

## 🚀 Next Steps

### Day 4 Preview
- **Performance & Monitoring**: API performance optimization
- **Deployment & DevOps**: CI/CD security practices
- **Load Testing**: Performance under stress
- **Monitoring**: Application performance monitoring

### Take-Home Assignment
- **Security Audit**: Perform security audit on personal projects
- **OAuth Implementation**: Add OAuth to a side project
- **Security Testing**: Run OWASP ZAP on a public API
- **Documentation**: Create security runbook for TaskFlow API

---

## 💡 Teaching Tips

### Engagement Strategies
- **Real-world examples**: Use recent security breaches as examples
- **Hands-on demos**: Show live security testing
- **Interactive discussions**: Encourage security-focused thinking
- **Practical exercises**: Build security into every activity

### Common Challenges
- **Complexity**: OAuth 2.0 can be overwhelming - break it down
- **Configuration**: Google OAuth setup can be tricky - provide step-by-step
- **Testing**: Security testing requires patience - encourage exploration
- **Mindset**: Shift from "it works" to "is it secure"

### Success Metrics
- **Implementation**: Students can implement OAuth 2.0
- **Understanding**: Students can explain security concepts
- **Application**: Students apply security to their own projects
- **Mindset**: Students think security-first in development

---

## 🚨 Emergency Procedures

### If OAuth Setup Fails
1. **Have backup demo ready**: Pre-configured Google OAuth
2. **Use alternative**: Show with mock OAuth implementation
3. **Focus on concepts**: Explain flow without live demo
4. **Provide resources**: Give students setup guides for later

### If Security Testing Tools Don't Work
1. **Use browser dev tools**: Manual security header checking
2. **Use Postman**: Manual security testing
3. **Use curl**: Command-line security testing
4. **Focus on concepts**: Explain what tools should find

### If Students Struggle with Concepts
1. **Use analogies**: Real-world security examples
2. **Break down complexity**: Step-by-step explanations
3. **Provide examples**: Show before/after code
4. **Encourage questions**: Create safe space for learning

---

## 📊 Day 3 Success Metrics

### Technical Implementation
- [ ] Google OAuth working in TaskFlow API
- [ ] Rate limiting implemented and tested
- [ ] Security headers configured and validated
- [ ] WebSocket authentication implemented
- [ ] Security testing tools demonstrated

### Knowledge Transfer
- [ ] Students understand OAuth 2.0 flows
- [ ] Students can identify OWASP Top 10 vulnerabilities
- [ ] Students implement security best practices
- [ ] Students think security-first in development

### Practical Application
- [ ] Students secure their own APIs
- [ ] Students perform security testing
- [ ] Students create security documentation
- [ ] Students understand production security requirements

---

*This facilitator guide ensures a comprehensive, hands-on Day 3 focused on production-grade API security implementation.* 