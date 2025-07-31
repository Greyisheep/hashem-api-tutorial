# Day 4 Facilitator Guide: API Monetization, Legacy Refactoring & Versioning

## 🎯 Session Overview

**Duration**: 8 hours (9:00 AM - 5:00 PM)  
**Focus**: Real-world API monetization, legacy code refactoring, and production-ready practices  
**Key Project**: Squad.co payment gateway integration with industry-standard patterns

## 📋 Detailed Agenda & Teaching Notes

### Morning Session (9:00 AM - 12:00 PM)

#### 9:00 - 9:30 AM: Welcome & Day Overview

**Teaching Objectives**:
- Review Days 1-3 achievements
- Set expectations for Day 4
- Introduce Squad.co integration project

**Key Points to Cover**:
```markdown
✅ Days 1-3 Review (5 minutes)
- RESTful API design principles mastered
- Authentication & authorization implemented
- Security best practices applied
- Basic CRUD operations working

🎯 Day 4 Preview (10 minutes)
- API monetization strategies
- Real-world payment gateway integration
- Legacy code refactoring techniques
- Production versioning practices

🛠️ Squad.co Project Overview (15 minutes)
- Payment gateway integration
- Security best practices
- Docker secrets management
- Rate limiting implementation
```

**Student Engagement**:
- Ask: "What challenges did you face with the previous days' assignments?"
- Poll: "How many have worked with payment gateways before?"
- Discuss: "What's the largest codebase you've worked with?"

#### 9:30 - 10:30 AM: API Monetization Strategies

**Teaching Objectives**:
- Understand different API revenue models
- Learn Squad.co integration patterns
- Implement payment gateway security

**Lecture Structure**:

**1. API Monetization Models (20 minutes)**
```markdown
💰 Revenue Models:
- Freemium (Stripe, Twilio)
- Usage-based (AWS, Google Cloud)
- Subscription (GitHub API, SendGrid)
- Transaction fees (Squad.co, PayPal)
- Enterprise licensing (Salesforce, Adobe)

📊 Pricing Strategies:
- Tiered pricing (Basic, Pro, Enterprise)
- Pay-per-call (API calls, requests)
- Revenue sharing (marketplace models)
- White-label licensing

🎯 Implementation Considerations:
- API key management
- Usage tracking
- Billing integration
- Rate limiting by tier
- Analytics and reporting
```

**2. Squad.co Integration Deep Dive (25 minutes)**
```markdown
🔐 Security First:
- API key management
- Webhook signature verification
- Encryption for sensitive data
- Rate limiting implementation

💳 Payment Flow:
1. Initialize payment
2. Redirect to Squad.co
3. User completes payment
4. Webhook notification
5. Payment verification
6. Business logic execution

🛡️ Security Measures:
- HTTPS enforcement
- Input validation
- SQL injection prevention
- XSS protection
- CSRF protection
```

**3. Hands-on Demo (15 minutes)**
- Show Squad.API implementation
- Demonstrate payment flow
- Explain security measures

**Student Activity**: Monetization Strategy Workshop
- Groups of 3-4 students
- Design monetization strategy for their API
- Present to class (5 minutes each)

#### 10:30 - 11:00 AM: Break

**Facilitator Tasks**:
- Prepare refactoring demo materials
- Check student progress on Squad.co setup
- Answer individual questions

#### 11:00 - 12:00 PM: Legacy Code Refactoring

**Teaching Objectives**:
- Break down large codebases systematically
- Apply Domain-Driven Design principles
- Implement industry-standard refactoring patterns

**Lecture Structure**:

**1. The Legacy Code Problem (15 minutes)**
```markdown
😱 Common Issues:
- 4000+ line files
- Tight coupling
- Mixed responsibilities
- No clear domain boundaries
- Difficult to test
- Hard to maintain

🎯 Refactoring Goals:
- Single Responsibility Principle
- Clear domain boundaries
- Loose coupling
- High cohesion
- Testable code
- Maintainable structure

📊 Impact Analysis:
- Business impact assessment
- Risk mitigation strategies
- Rollback plans
- Testing strategies
```

**2. Refactoring Strategies (20 minutes)**
```markdown
🔧 Extract Method Pattern:
- Identify code blocks
- Extract to private methods
- Improve readability
- Enable unit testing

🏗️ Extract Class Pattern:
- Group related methods
- Create new classes
- Define clear interfaces
- Reduce class size

📦 Extract Package/Module Pattern:
- Organize by domain
- Create clear boundaries
- Separate concerns
- Enable team ownership

🔄 Refactoring Steps:
1. Analyze current code
2. Identify responsibilities
3. Plan extraction strategy
4. Create tests first
5. Extract gradually
6. Verify functionality
7. Update documentation
```

**3. Domain-Driven Design (DDD) Introduction (15 minutes)**
```markdown
🏛️ DDD Layers:
- Domain Layer (business logic)
- Application Layer (use cases)
- Infrastructure Layer (external concerns)
- Presentation Layer (API controllers)

🎯 DDD Patterns:
- Entities (with identity)
- Value Objects (immutable)
- Aggregates (consistency boundaries)
- Repositories (data access)
- Services (domain services)

📋 Implementation Strategy:
- Start with bounded contexts
- Identify aggregates
- Define repositories
- Implement domain services
- Add application services
```

**4. Hands-on Refactoring Demo (10 minutes)**
- Show before/after examples
- Demonstrate step-by-step process
- Explain decision-making process

**Student Activity**: Code Decomposition Exercise
- Provide sample 4000-line file
- Students work in pairs
- Apply refactoring patterns
- Present results

### Afternoon Session (1:00 PM - 5:00 PM)

#### 1:00 - 2:30 PM: API Versioning & Backward Compatibility

**Teaching Objectives**:
- Understand versioning strategies
- Implement backward compatibility
- Handle breaking changes gracefully

**Lecture Structure**:

**1. Versioning Strategies (20 minutes)**
```markdown
🌐 URL Versioning:
- /api/v1/users
- /api/v2/users
- Clear and explicit
- Easy to understand

📋 Header Versioning:
- Accept: application/vnd.api+json;version=1
- Clean URLs
- More complex implementation

🔗 Media Type Versioning:
- Accept: application/vnd.company.v1+json
- RESTful approach
- Content negotiation

📝 Query Parameter Versioning:
- /api/users?version=1
- Simple implementation
- Less RESTful

🎯 Strategy Selection:
- Consider API complexity
- Evaluate client capabilities
- Plan for long-term maintenance
- Document versioning policy
```

**2. Backward Compatibility (25 minutes)**
```markdown
🔄 Compatibility Strategies:
- Additive changes only
- Deprecation warnings
- Graceful degradation
- Feature flags
- Multiple versions support

⚠️ Breaking Changes:
- Plan migration strategy
- Communicate timeline
- Provide migration guides
- Support multiple versions

📊 Version Lifecycle:
- Alpha (experimental)
- Beta (testing)
- Stable (production)
- Deprecated (sunset)
- Retired (removed)
```

**3. Implementation Patterns (30 minutes)**
```markdown
🏗️ Versioning Implementation:
- Version routing
- Response transformation
- Request validation
- Documentation management
- Testing strategies

🔧 Technical Implementation:
- Route constraints
- Middleware for versioning
- Response transformers
- Documentation generators
- Automated testing
```

**4. Squad.API Versioning Demo (15 minutes)**
- Show versioning implementation
- Demonstrate backward compatibility
- Explain migration strategy

**Student Activity**: Versioning Workshop
- Design versioning strategy for their API
- Implement version routing
- Handle breaking changes

#### 2:30 - 3:00 PM: Break

**Facilitator Tasks**:
- Prepare security demo
- Review student progress
- Set up rate limiting examples

#### 3:00 - 4:00 PM: Production Security & Rate Limiting

**Teaching Objectives**:
- Implement production-ready security
- Add rate limiting to APIs
- Monitor and protect against abuse

**Lecture Structure**:

**1. Security Best Practices (20 minutes)**
```markdown
🔒 Security Headers:
- Content-Security-Policy
- X-Frame-Options
- X-Content-Type-Options
- Strict-Transport-Security
- X-XSS-Protection

🛡️ Input Validation:
- Request validation
- SQL injection prevention
- XSS protection
- CSRF protection
- Input sanitization

🔐 Authentication & Authorization:
- JWT token management
- Role-based access control
- API key validation
- Session management
- Multi-factor authentication
```

**2. Rate Limiting Strategies (25 minutes)**
```markdown
⚡ Rate Limiting Algorithms:
- Token Bucket
- Leaky Bucket
- Fixed Window
- Sliding Window
- Adaptive Rate Limiting

📊 Implementation:
- In-memory storage
- Redis-based storage
- Distributed rate limiting
- Custom headers
- Error responses

🎯 Rate Limiting Policies:
- Per-user limits
- Per-IP limits
- Tier-based limits
- Burst handling
- Graceful degradation
```

**3. Monitoring & Alerting (15 minutes)**
```markdown
📈 Monitoring:
- Request metrics
- Error rates
- Response times
- Rate limit hits
- Security events

🚨 Alerting:
- Threshold-based alerts
- Anomaly detection
- Security incidents
- Performance degradation

📊 Analytics:
- Usage patterns
- Performance trends
- Error analysis
- Security monitoring
```

**4. Hands-on Security Demo (10 minutes)**
- Show rate limiting implementation
- Demonstrate security headers
- Explain monitoring setup

**Student Activity**: Security Implementation
- Add rate limiting to Squad.API
- Implement security headers
- Set up basic monitoring

#### 4:00 - 5:00 PM: Take-Home Assignment & Q&A

**Teaching Objectives**:
- Assign comprehensive homework
- Review key concepts
- Address remaining questions

**Assignment Overview**:
```markdown
📝 Take-Home Assignment:
1. Enhance Squad.API with rate limiting
2. Add comprehensive security headers
3. Implement monitoring and logging
4. Create API documentation
5. Add automated tests

🎯 Bonus Challenges:
- Circuit breaker pattern
- Caching with Redis
- CI/CD pipeline
- Performance optimization
- Comprehensive error handling
```

**Q&A Session**:
- Address student questions
- Clarify complex concepts
- Provide additional resources
- Preview Day 5 topics

## 🛠️ Technical Setup Guide

### Prerequisites Check
```bash
# Verify Docker installation
docker --version
docker-compose --version

# Check .NET SDK
dotnet --version

# Verify Squad.co setup
# Students should have API keys ready
```

### Squad.API Setup
```bash
# Navigate to Squad.API
cd Squad.API

# Create environment file
cp env.example .env

# Edit .env with Squad.co credentials
# (Instructor should provide test credentials)

# Start the application
docker-compose up -d

# Verify setup
curl http://localhost:5000/health
```

### Docker Secrets Management
```bash
# Create setup script
cat > setup-secrets.sh << 'EOF'
#!/bin/bash

# Create .env file with secrets
cat > .env << 'ENV_EOF'
SQUAD_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904
SQUAD_BASE_URL=https://sandbox-api-d.squadco.com
SQUAD_WEBHOOK_SECRET=cc984c0ceb8ee82e2b8bc5c60c33b69c
JWT_SECRET=fsdbhgevfugeilqgsfcnagvsuyqlXJ1lorkix_ownQGD
SQUAD_PUBLIC_KEY=sandbox_pk_4e0cb820488990016ea2bdd035cf18751b17f9026572
NOTIFICATION_EMAIL=greyisheep@gmail.com
JWT_ISSUER=https://localhost:5001
JWT_AUDIENCE=https://localhost:5001
JWT_EXPIRY=1h
BANK_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904
ACCOUNT_LOOKUP=https://sandbox-api-d.squadco.com/payout/account/lookup
FUND_TRANSFER=https://sandbox-api-d.squadco.com/payout/transfer
REQUERY_TRANSFER=https://sandbox-api-d.squadco.com/payout/requery
GET_ALL_TRANSFERS=https://sandbox-api-d.squadco.com/payout/list
SALT_VALUE=TGfdg542*&dltr51g
PASS_PHRASE=Jk6547fdrloln53@#qj
INIT_VECTOR=Goj0458jgte32vb0
PASSWORD_ITERATIONS=2
BLOCKSIZE=32
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=squad_db;Username=squad_user;Password=squad_password
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
ENV_EOF

# Remove the script after execution
rm setup-secrets.sh
EOF

chmod +x setup-secrets.sh
```

## 📚 Teaching Resources

### Code Examples
- `Squad.API/` - Complete payment gateway implementation
- `Day-4/demos/` - Refactoring examples
- `Day-4/exercises/` - Practice materials

### Documentation
- [Squad.co API Docs](https://docs.squadco.com/)
- [API Versioning Guide](https://restfulapi.net/versioning/)
- [DDD Patterns](https://martinfowler.com/bliki/DomainDrivenDesign.html)

### Tools & Libraries
- Squad.co .NET SDK
- FluentValidation
- Serilog
- Polly (resilience patterns)

## 🎯 Assessment Criteria

### Day 4 Success Metrics
1. ✅ Squad.co integration working end-to-end
2. ✅ Legacy code refactored using DDD principles
3. ✅ API versioning implemented correctly
4. ✅ Rate limiting and security measures in place
5. ✅ Docker secrets properly configured

### Student Evaluation
- **Participation**: Active engagement in workshops
- **Implementation**: Working Squad.co integration
- **Understanding**: Ability to explain concepts
- **Application**: Successfully refactoring code
- **Security**: Proper implementation of security measures

## 🆘 Troubleshooting Guide

### Common Issues

**Squad.co Integration Problems**:
```markdown
❌ API Key Issues:
- Verify sandbox vs production keys
- Check API key permissions
- Ensure proper base URL

❌ Webhook Issues:
- Verify webhook URL
- Check signature verification
- Test with webhook testing tools

❌ Payment Flow Issues:
- Check redirect URLs
- Verify callback handling
- Test with Squad.co test cards
```

**Docker Issues**:
```markdown
❌ Container Startup Problems:
- Check Docker logs
- Verify environment variables
- Ensure ports are available

❌ Database Connection Issues:
- Check PostgreSQL container
- Verify connection string
- Check network configuration
```

**Rate Limiting Issues**:
```markdown
❌ Rate Limiting Not Working:
- Check Redis connection
- Verify rate limiting configuration
- Test with multiple requests
```

## 📝 Additional Notes

### Teaching Tips
1. **Start with real-world examples** - Students relate better to practical applications
2. **Use visual aids** - Diagrams help explain complex concepts
3. **Encourage questions** - Create a safe environment for asking questions
4. **Provide hands-on time** - Students learn best by doing
5. **Connect to previous days** - Build on existing knowledge

### Time Management
- **Morning**: Focus on concepts and Squad.co integration
- **Afternoon**: Emphasize practical implementation
- **End of day**: Ensure students have working examples

### Student Support
- **Office hours**: Available for individual help
- **Slack channel**: Real-time support
- **Documentation**: Comprehensive guides provided
- **Code examples**: Working implementations available

---

**Remember**: Day 4 is about bridging theory with real-world application. Focus on practical skills that students can immediately apply to their projects! 