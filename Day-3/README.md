# Day 3: API Security Best Practices

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
**Activity**: "What did you learn about OWASP and OAuth 2.0 from your reading?"
**Your Role**: Assess knowledge gaps, set security mindset

#### Pre-flight Checklist:
- [ ] All students have Google Cloud Console access (or demo accounts ready)
- [ ] TaskFlow API running with current security baseline
- [ ] Postman collections updated for security testing
- [ ] OWASP ZAP or similar security testing tool ready
- [ ] Sticky notes for security threat modeling
- [ ] Timer visible to group

#### Opening Script:
"Yesterday we built APIs. Today we secure them. Let's start with what you learned about OWASP and OAuth 2.0. What security concerns keep you up at night?"

### 10:30-10:45 AM: Break (15 min)
**Your prep time**: Set up Google OAuth demo environment

### 10:45-12:00 PM: Module 1 - OAuth 2.0 Implementation (75 min)

#### OAuth 2.0 Deep Dive (20 min)
**Materials**: [`demos/oauth2-implementation-demo.md`](./demos/oauth2-implementation-demo.md)
- **Show**: Live OAuth 2.0 flow with Google
- **Focus**: Authorization Code Flow with PKCE
- **Key Learning**: Why PKCE matters for SPAs and mobile apps

#### Google OAuth Setup Walkthrough (25 min)
**Activity**: Step-by-step Google OAuth configuration
- **Setup**: Google Cloud Console project creation
- **Configuration**: OAuth 2.0 client setup
- **Integration**: .NET Identity with Google provider
- **Testing**: Complete login flow

#### OAuth Implementation in TaskFlow (30 min)
**Materials**: [`implementation/oauth2-setup-guide.md`](./implementation/oauth2-setup-guide.md)
- **Code-along**: Add Google OAuth to TaskFlow API
- **Security**: Proper token validation and refresh
- **Testing**: Postman OAuth flow testing

### 12:00-1:00 PM: Lunch (60 min)
**Your prep time**: Prepare rate limiting and security headers implementation

### 1:00-2:15 PM: Module 2 - Rate Limiting & Security Headers (75 min)

#### Rate Limiting Fundamentals (20 min)
**Discussion**: "Why do we need rate limiting?"
- **Threats**: Brute force, scraping, DoS attacks
- **Strategies**: Fixed window, sliding window, token bucket
- **Implementation**: Middleware vs API Gateway

#### Rate Limiting Implementation (30 min)
**Materials**: [`implementation/rate-limiting-guide.md`](./implementation/rate-limiting-guide.md)
- **Code-along**: Add rate limiting to TaskFlow API
- **Configuration**: Per-user, per-IP, per-endpoint limits
- **Monitoring**: Rate limit headers and logging

#### Security Headers & Middleware (25 min)
**Materials**: [`implementation/security-headers-guide.md`](./implementation/security-headers-guide.md)
- **Implementation**: Comprehensive security headers
- **CORS**: Proper cross-origin configuration
- **CSP**: Content Security Policy setup
- **Testing**: Security header validation

### 2:15-2:30 PM: Break (15 min)
**Your prep time**: Prepare WebSocket and webhook security demos

### 2:30-3:45 PM: Module 3 - Advanced Security Patterns (75 min)

#### Authenticated WebSockets (25 min)
**Materials**: [`demos/websocket-security-demo.md`](./demos/websocket-security-demo.md)
- **Implementation**: JWT-based WebSocket authentication
- **Security**: Token validation in WebSocket connections
- **Demo**: Real-time task updates with authentication

#### Secure Webhook Implementation (25 min)
**Materials**: [`implementation/webhook-security-guide.md`](./implementation/webhook-security-guide.md)
- **Security**: Webhook signature validation
- **Implementation**: HMAC-based webhook verification
- **Testing**: Webhook security testing

#### Secrets Management (25 min)
**Materials**: [`implementation/secrets-management-guide.md`](./implementation/secrets-management-guide.md)
- **Azure Key Vault**: Integration with TaskFlow API
- **Environment**: Secure configuration management
- **Best Practices**: Never commit secrets to source code

### 3:45-4:00 PM: Break (15 min)
**Your prep time**: Prepare security testing and final exercises

### 4:00-5:00 PM: Module 4 - Security Testing & OWASP Top 10 (60 min)

#### OWASP API Security Top 10 Review (20 min)
**Materials**: [`exercises/owasp-top10-workshop.md`](./exercises/owasp-top10-workshop.md)
- **Interactive**: Students identify vulnerabilities in code
- **Discussion**: Real-world attack scenarios
- **Mitigation**: How to prevent each vulnerability

#### Security Testing Workshop (25 min)
**Activity**: Hands-on security testing
- **Tools**: OWASP ZAP, Postman security testing
- **Scenarios**: SQL injection, XSS, broken authentication
- **Reporting**: Security vulnerability documentation

#### Production Security Checklist (15 min)
**Materials**: [`exercises/production-security-checklist.md`](./exercises/production-security-checklist.md)
- **Review**: Comprehensive security checklist
- **Implementation**: Apply to TaskFlow API
- **Documentation**: Security runbook creation

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