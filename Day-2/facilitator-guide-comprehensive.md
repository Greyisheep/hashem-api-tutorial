# Day 2 Facilitator Guide - API Implementation & Architecture

## 🎯 Day 2 Objectives
By 4:00 PM, students should be able to:
- [ ] Review and improve takehome assignments from Day 1
- [ ] Conduct effective code reviews with domain focus
- [ ] Design and implement proper project structure and architecture
- [ ] Create database schemas using dbdiagram.io
- [ ] Implement Authentication & Authorization (OAuth 2.0, JWT, API keys)
- [ ] Apply production-ready patterns

**Key Deliverable**: Each student has a working API with proper architecture, database design, and authentication system.

---

## ⏰ Detailed Schedule & Timing

### 10:00-10:30 AM: Takehome Review & Production Setup (30 min)
**Activity**: "Show us your best API design from yesterday's takehome"
**Your Role**: Connect Day 1 concepts to today's implementation focus

#### Pre-flight Checklist:
- [ ] All students have .NET 8 SDK installed
- [ ] Docker Desktop running and accessible
- [ ] TaskFlow API repository cloned and ready
- [ ] Postman collections imported
- [ ] VS Code with C# extensions
- [ ] dbdiagram.io accounts ready
- [ ] Timer visible to group

#### Opening Script:
"Yesterday we designed APIs. Today we build them. Let's see how those domain models translate into working code!"

#### Takehome Review (20 min)
**Format**: Each student presents their best API design (3 min each)
- **Focus**: Domain language, REST principles, error handling
- **Your Role**: Ask probing questions, highlight good patterns
- **Key Questions**:
  - "How did you handle cross-domain operations?"
    <details><summary>**Answer**: Look for domain events, saga patterns, or orchestration services</summary>
    - Domain events for loose coupling between domains
    - Saga pattern for distributed transactions
    - Orchestration services to coordinate cross-domain operations
    - Event sourcing for audit trails
    </details>
  - "What error scenarios did you consider?"
    <details><summary>**Answer**: Network failures, validation errors, business rule violations</summary>
    - Network timeouts and retries
    - Input validation (400 errors)
    - Business rule violations (422 errors)
    - Authentication/authorization failures (401/403)
    - Server errors (500) with proper logging
    </details>
  - "How would this scale to 1000+ users?"
    <details><summary>**Answer**: Caching, database optimization, horizontal scaling</summary>
    - Redis caching for frequently accessed data
    - Database indexing and query optimization
    - Horizontal scaling with load balancers
    - Microservices architecture for independent scaling
    - CDN for static content
    </details>

### 10:30-10:45 AM: Break (15 min)
**Your prep time**: Set up TaskFlow API demo environment

### 10:45-11:45 AM: Module 1 - Project Structure & Architecture (60 min)

#### Architecture Walkthrough (20 min)
**Materials**: `taskflow-api-dotnet/DDD-IMPLEMENTATION-SUMMARY.md`
- **Show**: Live running TaskFlow API
- **Focus**: "See how yesterday's domain model became this code"
- **Walk through**: Value objects, aggregates, repositories
- **Energy**: "This is what production DDD looks like"

#### Code Review Session (25 min)
**Activity**: Code walkthrough with student participation
- **Start**: Domain layer - "What business rules do you see?"
- **Move**: Application layer - "How does CQRS work here?"
- **End**: Infrastructure - "Why this abstraction?"

**Key Questions**:
- "Why is Email a value object, not a string?"
  <details><summary>**Answer**: Encapsulation, validation, and domain logic</summary>
  - Email validation logic is encapsulated within the value object
  - Prevents invalid email states throughout the application
  - Domain rules (like email format) are enforced at the domain level
  - Makes the code more expressive and self-documenting
  - Easier to change email validation rules in one place
  </details>
- "What happens if we try to change a task's status?"
  <details><summary>**Answer**: Domain events and business rules are triggered</summary>
  - TaskStatusChangedEvent is raised and published
  - Business rules validate the status transition (e.g., can't move from "Completed" to "In Progress")
  - Event handlers can trigger notifications, audit logs, or other side effects
  - Repository pattern ensures data consistency
  - Domain invariants are maintained
  </details>
- "How does the repository pattern help us?"
  <details><summary>**Answer**: Abstraction, testability, and domain focus</summary>
  - Abstracts data access details from domain logic
  - Makes unit testing easier with mock repositories
  - Domain entities focus on business rules, not data access
  - Can easily switch between different data stores (SQL, NoSQL, etc.)
  - Centralizes data access logic and query optimization
  </details>

#### Project Structure Deep Dive (15 min)
**Show**: Complete project organization
- **Domain Layer**: Entities, value objects, domain events
- **Application Layer**: Commands, queries, services
- **Infrastructure Layer**: Repositories, external services
- **API Layer**: Controllers, middleware, configuration

### 11:45-12:00 PM: Break (15 min)
**Your prep time**: Prepare database design session

### 12:00-1:00 PM: Module 2 - Database Design with dbdiagram.io (60 min)

#### Database Design Principles (15 min)
**Discussion**: "How do you translate domain models to database schemas?"
- **Show**: Domain entities → Database tables
- **Focus**: Relationships, constraints, indexing
- **Key insight**: "Database design reflects domain boundaries"

#### dbdiagram.io Workshop (30 min)
**Activity**: Design TaskFlow database schema
- **Setup**: Everyone creates dbdiagram.io account
- **Task**: Design complete TaskFlow database
- **Include**: Users, Projects, Tasks, Teams, Permissions
- **Focus**: Relationships, foreign keys, indexes

**Your Role**: Walk around, help with relationships, validate designs

#### Schema Review & Optimization (15 min)
**Activity**: Review each other's schemas
- **Pair up**: Students review partner's design
- **Focus**: Performance, normalization, relationships
- **Key Questions**:
  - "How would this handle 10,000 tasks?"
    <details><summary>**Answer**: Pagination, indexing, and query optimization</summary>
    - Implement pagination (limit/offset or cursor-based)
    - Add indexes on frequently queried columns (user_id, project_id, status, created_at)
    - Use database partitioning for large tables
    - Implement caching for frequently accessed data
    - Consider read replicas for heavy read workloads
    </details>
  - "What indexes would you add?"
    <details><summary>**Answer**: Composite indexes based on query patterns</summary>
    - Primary key indexes (automatic)
    - Foreign key indexes (user_id, project_id)
    - Composite indexes: (user_id, status), (project_id, created_at)
    - Unique indexes: email, username
    - Partial indexes: WHERE status = 'active'
    - Consider covering indexes for frequently accessed queries
    </details>
  - "How do you handle soft deletes?"
    <details><summary>**Answer**: Deleted_at column and query filtering</summary>
    - Add `deleted_at` timestamp column (nullable)
    - Set `deleted_at = NOW()` instead of actually deleting
    - Filter queries with `WHERE deleted_at IS NULL`
    - Create database views for "active" records
    - Consider archive tables for very old deleted records
    - Implement hard delete for GDPR compliance when needed
    </details>

### 1:00-2:00 PM: Lunch (60 min)
**Your prep time**: Set up authentication demo environment

### 2:00-3:30 PM: Module 3 - Authentication & Authorization (90 min)

#### Authentication Fundamentals (20 min)
**Discussion**: "What are the different ways to authenticate API users?"
- **Show**: OAuth 2.0 flow diagrams
- **Compare**: JWT vs Session tokens
- **Key insight**: "Choose based on your use case"
- **Key Questions**:
  - "When would you use JWT vs Session tokens?"
    <details><summary>**Answer**: JWT for stateless APIs, Sessions for stateful apps</summary>
    - JWT: Stateless, good for microservices, mobile apps, single-page apps
    - Sessions: Stateful, better for traditional web apps, easier to revoke
    - JWT: Larger payload but no server storage needed
    - Sessions: Smaller payload but requires server-side storage
    - JWT: Harder to revoke (need blacklisting or short expiration)
    - Sessions: Easy to revoke by deleting server-side session
    </details>
  - "What's the difference between authentication and authorization?"
    <details><summary>**Answer**: Authentication = who you are, Authorization = what you can do</summary>
    - Authentication: Verifies identity (login, password, biometrics)
    - Authorization: Determines permissions (roles, claims, policies)
    - Authentication happens first, then authorization
    - JWT tokens can contain both identity and permission claims
    - Role-based access control (RBAC) is an authorization pattern
    - Claims-based authorization is more flexible than roles
    </details>

#### OAuth 2.0 Implementation (30 min)
**Demo**: Complete OAuth 2.0 flow
- **Show**: Authorization code flow
- **Demonstrate**: Token exchange
- **Focus**: Security considerations

#### JWT Implementation (25 min)
**Code-Along**: Implement JWT authentication
- **Setup**: JWT token generation and validation
- **Show**: Token structure and claims
- **Focus**: Security best practices

#### API Keys & Authorization (15 min)
**Discussion**: "When do you use API keys vs OAuth?"
- **Show**: API key implementation
- **Demonstrate**: Rate limiting with API keys
- **Key insight**: "API keys for machine-to-machine, OAuth for user access"
- **Key Questions**:
  - "When should you use API keys vs OAuth 2.0?"
    <details><summary>**Answer**: API keys for M2M, OAuth for user access</summary>
    - API Keys: Machine-to-machine communication, server-to-server APIs
    - OAuth 2.0: User authentication, third-party app access
    - API Keys: Simpler, faster, but less secure
    - OAuth 2.0: More secure, supports user consent, refresh tokens
    - API Keys: Good for internal services, webhooks, background jobs
    - OAuth 2.0: Good for user-facing applications, mobile apps
    </details>
  - "How do you secure API keys?"
    <details><summary>**Answer**: Environment variables, rotation, and scoping</summary>
    - Store in environment variables, never in code
    - Use key rotation (change keys periodically)
    - Scope keys to specific permissions/endpoints
    - Use HTTPS for all API key transmission
    - Monitor for unusual usage patterns
    - Implement rate limiting per API key
    - Use different keys for different environments (dev/staging/prod)
    </details>

### 3:30-3:45 PM: Break (15 min)
**Your prep time**: Prepare final implementation session

### 3:45-4:00 PM: Module 4 - Integration & Next Steps (15 min)

#### Implementation Integration (10 min)
**Activity**: Connect all pieces together
- **Show**: Complete system with auth
- **Demonstrate**: Database + API + Authentication
- **Focus**: End-to-end flow

#### Day 3 Preview (5 min)
**Preview**: Security deep dive and production deployment
- **Teaser**: "Tomorrow we lock this down and deploy it"

---

## 🎯 Facilitation Strategies

### For Implementation-Focused Day:
- **Hands-on priority**: More coding, less talking
- **Pair programming**: Students work together on exercises
- **Live debugging**: Use real errors as teaching moments
- **Celebrate wins**: "Look what you just built!"

### Energy Management:
- **Morning**: High energy with takehome review
- **Mid-morning**: Focus on architecture and structure
- **Afternoon**: Database design excitement
- **Late afternoon**: Authentication implementation
- **End**: Integration satisfaction

### Common Implementation Issues:
- **Docker problems**: "Let's debug this together"
- **Code errors**: "What's the error telling us?"
- **Database confusion**: "Let's draw this out"

---

## 🧪 Hands-on Exercises

### Exercise 1: Database Design (30 min)
**Objective**: Design complete TaskFlow database schema

**Success Criteria**:
- [ ] All domain entities represented
- [ ] Proper relationships defined
- [ ] Appropriate indexes identified
- [ ] Performance considerations noted

**Your Role**: Help with relationships, validate designs

### Exercise 2: Authentication Implementation (45 min)
**Objective**: Implement JWT authentication in TaskFlow API

**Success Criteria**:
- [ ] JWT token generation works
- [ ] Token validation implemented
- [ ] Protected endpoints secured
- [ ] Error handling in place

**Your Role**: Help debug, guide security thinking

### Exercise 3: Code Review Practice (20 min)
**Objective**: Practice effective code review

**Success Criteria**:
- [ ] Domain-focused feedback given
- [ ] Security issues identified
- [ ] Performance concerns raised
- [ ] Positive feedback balanced

**Your Role**: Model good review practices

---

## 📊 Assessment & Validation

### During Implementation:
**Look for**:
- [ ] Understanding of DDD patterns
- [ ] Proper database design
- [ ] Security implementation
- [ ] Code review skills

**Red flags**:
- [ ] Copy-paste without understanding
- [ ] Ignoring security considerations
- [ ] Poor database relationships
- [ ] No code review participation

### End of Day Checklist:
**Each student should have**:
- [ ] Reviewed takehome assignments
- [ ] Understanding of project architecture
- [ ] Database schema designed
- [ ] Authentication implemented
- [ ] Code review experience
- [ ] Clear next steps for Day 3

---

## 🛠️ Technical Setup & Troubleshooting

### Common Issues:
**Docker problems**: `docker-compose down && docker-compose up --build`
**Database issues**: Reset with `docker-compose down -v`
**API issues**: Check logs with `docker-compose logs taskflow-api`
**dbdiagram.io issues**: Use backup browser or mobile app

### Common C#/.NET Questions & Answers:
- **"What's the difference between `var` and explicit types?"**
  <details><summary>**Answer**: `var` for type inference, explicit for clarity</summary>
  - `var` lets the compiler infer the type from the right side
  - Use `var` when the type is obvious from context
  - Use explicit types when it adds clarity or for primitive types
  - `var` is compile-time only, doesn't affect runtime performance
  - Example: `var tasks = await _repository.GetAllAsync();` vs `List<Task> tasks = ...`
  </details>

- **"What's the difference between `async/await` and `Task.Run`?"**
  <details><summary>**Answer**: `async/await` for I/O, `Task.Run` for CPU-bound work</summary>
  - `async/await`: For I/O operations (database, HTTP, file operations)
  - `Task.Run`: For CPU-intensive work that should run on background thread
  - `async/await` doesn't create new threads, just frees up the current thread
  - `Task.Run` creates a new thread from the thread pool
  - Example: `await _dbContext.SaveChangesAsync()` vs `Task.Run(() => HeavyCalculation())`
  </details>

- **"What's dependency injection and why use it?"**
  <details><summary>**Answer**: Loose coupling, testability, and lifecycle management</summary>
  - DI provides objects with their dependencies instead of creating them
  - Makes code more testable (can inject mocks)
  - Manages object lifecycles (singleton, scoped, transient)
  - Reduces coupling between classes
  - Built into ASP.NET Core with `IServiceCollection`
  - Example: Constructor injection `public TaskController(ITaskRepository repository)`
  </details>

- **"What's the difference between `IEnumerable`, `IQueryable`, and `List`?"**
  <details><summary>**Answer**: Different levels of data access and execution</summary>
  - `IEnumerable`: In-memory collection, executes immediately
  - `IQueryable`: Database query builder, executes when enumerated
  - `List`: Concrete implementation, in-memory with random access
  - `IQueryable` is more efficient for database queries (filters at DB level)
  - `IEnumerable` is better for in-memory operations
  - Example: `IQueryable<Task>` for EF Core queries, `List<Task>` for results
  </details>

### Backup Plans:
**Demo failures**: Use pre-recorded videos
**Environment issues**: Use cloud-based alternatives
**Time pressure**: Focus on core authentication implementation
**Technical difficulties**: Pair students for support

---

## 📊 Success Metrics

### Immediate (End of Day 2):
- [ ] Students can conduct effective code reviews
- [ ] Can design proper database schemas
- [ ] Understand authentication patterns
- [ ] Can implement JWT authentication
- [ ] Have working project architecture

### Forward-looking (Setup for Day 3):
- [ ] Students confident with implementation
- [ ] Ready for security deep dive
- [ ] Understand production deployment
- [ ] Excited about advanced topics

---

## 🎪 Instructor Energy & Tone

### Maintain Implementation Focus:
- **"Let's build this!"**: Hands-on energy
- **"What's the error telling us?"**: Debugging mindset
- **"Look what you just created!"**: Celebration
- **"How would this work in production?"**: Real-world thinking

### Avoid:
- **Theory overload**: Keep concepts tied to implementation
- **Perfect code**: Focus on working solutions
- **Solo coding**: Encourage collaboration
- **Rushing**: Better working code than rushed code

### Recovery Strategies:
**If behind**: Skip optional demos, focus on core authentication
**If ahead**: Deeper implementation challenges
**If stuck**: Pair programming, group debugging
**If confused**: Stop, simplify, rebuild

---

**Remember**: Today is about building, not just learning. Students should leave with working code and confidence in their implementation skills.

**Day 2 Success**: Students have built, reviewed, and secured real APIs using production patterns. 