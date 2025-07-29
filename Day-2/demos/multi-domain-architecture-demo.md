# Multi-Domain Architecture Demo - TaskFlow .NET 9

## 🎯 Demo Objectives
- Show how to structure enterprise APIs with multiple domains
- Demonstrate Domain-Driven Design principles in .NET 9
- Illustrate proper domain boundaries and interactions
- Showcase database design for multi-domain systems

## 📁 Domain Structure Overview

```
TaskFlow.API/
├── src/
│   ├── TaskFlow.API/                    # Web API layer
│   ├── TaskFlow.SharedKernel/           # Shared domain primitives
│   ├── TaskFlow.Users.Domain/           # User management domain
│   ├── TaskFlow.Users.Application/      # User use cases
│   ├── TaskFlow.Users.Infrastructure/   # User data access
│   ├── TaskFlow.Projects.Domain/        # Project management domain
│   ├── TaskFlow.Projects.Application/   # Project use cases
│   ├── TaskFlow.Projects.Infrastructure/# Project data access
│   ├── TaskFlow.Tasks.Domain/           # Task management domain
│   ├── TaskFlow.Tasks.Application/      # Task use cases
│   ├── TaskFlow.Tasks.Infrastructure/   # Task data access
│   ├── TaskFlow.Comments.Domain/        # Comments domain
│   ├── TaskFlow.Comments.Application/   # Comments use cases
│   ├── TaskFlow.Comments.Infrastructure/# Comments data access
│   └── TaskFlow.Notifications.Domain/   # Notifications domain
└── database/
    └── taskflow-schema.dbml             # Complete database design
```

## 🏗️ Demo Script

### Part 1: Domain Discovery (5 minutes)

**Start with business capabilities:**
"Let's look at TaskFlow as a business. What are the core capabilities?"

1. **User Management**: Registration, authentication, profiles, roles
2. **Project Management**: Creating projects, managing members, lifecycle
3. **Task Management**: Creating tasks, assignment, tracking, completion
4. **Communication**: Comments, discussions, feedback
5. **Notifications**: System alerts, task updates, mentions

**Key Learning**: Domains are about business capabilities, not technical concerns.

### Part 2: Domain Boundaries (10 minutes)

**Show the criteria for domain boundaries:**

```csharp
// ✅ GOOD: Clear domain boundary
namespace TaskFlow.Users.Domain;

public class User : AggregateRoot
{
    // User-specific business logic
    public void ChangePassword(string newPasswordHash) { }
    public void UpdateProfile(string firstName, string lastName) { }
    public void AssignRole(UserRole role) { }
}

// ✅ GOOD: Separate domain boundary  
namespace TaskFlow.Projects.Domain;

public class Project : AggregateRoot
{
    // Project-specific business logic
    public void AddMember(UserId userId) { }
    public void Start() { }
    public void Complete() { }
}
```

**Anti-Pattern to Avoid:**
```csharp
// ❌ BAD: Mixed responsibilities
public class ProjectUser : AggregateRoot
{
    // Mixing user and project concerns
    public void ChangePassword() { } // User concern
    public void AddToProject() { }   // Project concern
}
```

### Part 3: Domain Interactions (10 minutes)

**Show how domains communicate:**

```csharp
// Domain Event - Published by Tasks domain
public record TaskAssignedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string AssigneeId { get; }
    public string ProjectId { get; }
    public DateTime OccurredOn { get; }
}

// Event Handler - In Notifications domain
public class TaskAssignedNotificationHandler 
    : INotificationHandler<TaskAssignedEvent>
{
    public async Task Handle(TaskAssignedEvent @event, 
        CancellationToken cancellationToken)
    {
        // Create notification for task assignment
        var notification = new Notification(
            type: "task_assigned",
            recipientId: @event.AssigneeId,
            message: $"You have been assigned task {@event.TaskId}"
        );
        
        await _notificationRepository.AddAsync(notification);
    }
}
```

**Key Learning**: Domains communicate through events, not direct calls.

### Part 4: Database Design Integration (15 minutes)

**Show dbdiagram.io integration:**

1. **Open dbdiagram.io**
2. **Import the TaskFlow schema:**
   ```sql
   // Copy from database/taskflow-database-design.dbml
   ```

3. **Show domain-to-table mapping:**
   - Users Domain → `users`, `user_sessions`, `api_keys`
   - Projects Domain → `projects`, `project_members`
   - Tasks Domain → `tasks`
   - Comments Domain → `comments`
   - Notifications Domain → `notifications`
   - Shared → `audit_logs`, `request_logs`

4. **Highlight relationships:**
   - Foreign keys respect domain boundaries
   - Cross-domain relationships through IDs only
   - Audit trails for compliance

### Part 5: .NET 9 Implementation (15 minutes)

**Show modern .NET 9 patterns:**

```csharp
// Program.cs - Dependency injection by domain
var builder = WebApplication.CreateBuilder(args);

// Register domain services
builder.Services.AddUsersDomain(builder.Configuration);
builder.Services.AddProjectsDomain(builder.Configuration);
builder.Services.AddTasksDomain(builder.Configuration);
builder.Services.AddCommentsDomain(builder.Configuration);
builder.Services.AddNotificationsDomain(builder.Configuration);

// Shared infrastructure
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure domain-specific endpoints
app.MapUsersEndpoints();
app.MapProjectsEndpoints();
app.MapTasksEndpoints();
app.MapCommentsEndpoints();
app.MapNotificationsEndpoints();

app.Run();
```

**Domain Registration Extension:**
```csharp
// TaskFlow.Users.Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddUsersDomain(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register domain services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        
        // Register command/query handlers
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));
        
        // Register domain event handlers
        services.AddScoped<INotificationHandler<UserCreatedEvent>, 
            UserCreatedNotificationHandler>();
        
        return services;
    }
}
```

## 🔍 Key Discussion Points

### 1. Domain Autonomy
**Question**: "How do you handle operations that span multiple domains?"

**Answer**: 
- Use domain events for eventual consistency
- Create application services for orchestration
- Avoid direct domain-to-domain calls

### 2. Shared Data
**Question**: "What about data that multiple domains need?"

**Answer**:
- Shared Kernel for common value objects (Email, Money, etc.)
- Reference data can be duplicated across domains
- Event sourcing for audit trails

### 3. Database Boundaries
**Question**: "Should each domain have its own database?"

**Answer**:
- Start with single database, separate schemas
- Move to separate databases when scaling requires it
- Consider transaction boundaries and consistency needs

## 🚨 Common Pitfalls to Address

### 1. Over-Engineering
"Don't create domains for every entity. Look for business capabilities and lifecycle boundaries."

### 2. Chatty Domains
"If domains are constantly talking to each other, reconsider the boundaries."

### 3. Shared Database Anti-Pattern
"Just because domains share a database doesn't mean they can directly access each other's tables."

## 🎯 Demo Outcomes

By the end of this demo, students should understand:
- How to identify domain boundaries in business requirements
- How to structure .NET projects for multi-domain architecture
- How domains communicate through events
- How to design databases that support domain autonomy
- Modern .NET 9 dependency injection patterns

## 📝 Follow-up Exercise

**Hands-on Challenge**: "Take 15 minutes to identify where you would add a 'Reports' domain to TaskFlow. What would it contain? How would it get its data?"

**Expected Discussion**:
- Reports domain would aggregate data from other domains
- Would subscribe to domain events for data collection
- Might have its own read models for performance
- Would not directly access other domains' data stores

## 🔗 Related Materials

- [`exercises/domain-boundary-exercise.md`](../exercises/domain-boundary-exercise.md)
- [`implementation/ddd-project-structure.md`](../implementation/ddd-project-structure.md)
- [`database/taskflow-database-design.dbml`](../../taskflow-api-dotnet/database/taskflow-database-design.dbml)

---

**Remember**: The goal is to show how DDD principles scale to real enterprise applications. Keep the focus on business value and practical implementation, not theoretical purity. 