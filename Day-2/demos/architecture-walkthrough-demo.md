# Architecture Walkthrough Demo - TaskFlow API

## 🎯 Demo Objectives
- Show live TaskFlow API with DDD implementation
- Walk through project structure and architecture
- Demonstrate domain-driven design in action
- Connect Day 1 concepts to real implementation

## ⏰ Demo Timing: 20 minutes

---

## 🚀 Setup Instructions

### Prerequisites
- TaskFlow API running locally
- Docker containers started
- Postman collections ready
- Database seeded with sample data

### Quick Start
```bash
# Navigate to TaskFlow API
cd taskflow-api-dotnet

# Start the application
docker-compose up --build

# Verify it's running
curl http://localhost:5000/health
```

---

## 🏗️ Architecture Overview (5 minutes)

### Project Structure Walkthrough
**Show**: Complete project structure in VS Code

```
taskflow-api-dotnet/
├── src/
│   ├── TaskFlow.Domain/           # Business logic, entities, value objects
│   │   ├── Entities/
│   │   │   ├── User.cs           # User aggregate root
│   │   │   ├── Project.cs        # Project aggregate root
│   │   │   └── Task.cs           # Task aggregate root
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs          # Email value object
│   │   │   ├── TaskTitle.cs      # TaskTitle value object
│   │   │   ├── ProjectName.cs    # ProjectName value object
│   │   │   ├── UserRole.cs       # UserRole value object
│   │   │   └── TaskStatus.cs     # TaskStatus value object
│   │   ├── DomainEvents/
│   │   │   ├── UserCreatedEvent.cs
│   │   │   └── TaskAssignedEvent.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   ├── TaskFlow.Application/      # Use cases, DTOs, interfaces
│   │   ├── Commands/
│   │   │   └── CreateTask/
│   │   │       ├── CreateTaskCommand.cs
│   │   │       └── CreateTaskCommandHandler.cs
│   │   ├── Queries/
│   │   │   └── GetAllTasks/
│   │   │       ├── GetAllTasksQuery.cs
│   │   │       └── GetAllTasksQueryHandler.cs
│   │   ├── DTOs/
│   │   │   └── TaskDto.cs
│   │   └── Interfaces/
│   │       ├── ITaskRepository.cs
│   │       └── IUserRepository.cs
│   ├── TaskFlow.Infrastructure/   # Data access, external services
│   │   ├── Persistence/
│   │   │   ├── TaskFlowDbContext.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── TaskRepository.cs
│   │   │   │   └── UserRepository.cs
│   │   │   └── Configurations/
│   │   │       ├── TaskConfiguration.cs
│   │   │       └── UserConfiguration.cs
│   │   └── Services/
│   │       └── JwtService.cs
│   └── TaskFlow.API/             # Controllers, middleware, configuration
│       ├── Controllers/
│       │   ├── TasksController.cs
│       │   ├── UsersController.cs
│       │   ├── ProjectsController.cs
│       │   └── AuthController.cs
│       ├── Middleware/
│       │   └── ErrorHandlingMiddleware.cs
│       └── Program.cs
├── database/
│   └── init/
│       └── taskflow-database-design.dbml
├── postman/
│   ├── TaskFlow-API.postman_collection.json
│   └── TaskFlow-API.postman_environment.json
└── DDD-IMPLEMENTATION-SUMMARY.md
```

### Key Architecture Points:
1. **Domain Layer**: Pure business logic, no dependencies
2. **Application Layer**: Use cases and orchestration
3. **Infrastructure Layer**: Data access and external services
4. **API Layer**: Controllers and HTTP concerns

---

## 🏛️ Domain Layer Deep Dive (5 minutes)

### Value Objects in Action
**Show**: Email value object implementation from `src/TaskFlow.Domain/ValueObjects/Email.cs`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/Email.cs
public class Email : ValueObject
{
    public string Value { get; }
    
    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");
        
        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");
            
        Value = value.ToLowerInvariant().Trim();
    }
    
    public static Email From(string value) => new(value);
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

**Key Points**:
- **Encapsulation**: Email validation logic inside the value object
- **Immutability**: Value cannot be changed after creation
- **Business Rules**: Enforces email format requirements
- **Equality**: Two emails are equal if they have the same value

### Aggregate Root Example
**Show**: Task aggregate root from `src/TaskFlow.Domain/Entities/Task.cs`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.Domain/Entities/Task.cs
public class Task : AggregateRoot
{
    public TaskId Id { get; private set; }
    public TaskTitle Title { get; private set; }
    public TaskDescription Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public UserId AssigneeId { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // Private constructor for EF Core
    private Task() { }
    
    public Task(TaskTitle title, TaskDescription description, ProjectId projectId, UserId? assigneeId = null)
    {
        Id = TaskId.New();
        Title = title ?? throw new DomainException("Task title cannot be null");
        Description = description ?? throw new DomainException("Task description cannot be null");
        ProjectId = projectId ?? throw new DomainException("Project ID cannot be null");
        AssigneeId = assigneeId;
        Status = TaskStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new TaskCreatedEvent(Id, Title.Value, ProjectId.Value));
    }
    
    // Business Logic Methods
    public void AssignTo(UserId assigneeId)
    {
        if (Status == TaskStatus.Completed)
            throw new DomainException("Cannot assign completed tasks");
            
        AssigneeId = assigneeId ?? throw new DomainException("Assignee ID cannot be null");
        Status = TaskStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new TaskAssignedEvent(Id, assigneeId.Value));
    }
    
    public void Complete()
    {
        if (Status == TaskStatus.Completed)
            throw new DomainException("Task is already completed");
            
        Status = TaskStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new TaskCompletedEvent(Id));
    }
}
```

**Key Points**:
- **Business Logic**: Methods enforce business rules
- **Domain Events**: Important state changes trigger events
- **Encapsulation**: Internal state protected, changed only through methods
- **Validation**: Business rules enforced at domain level

---

## ⚙️ Application Layer Walkthrough (5 minutes)

### CQRS Pattern in Action
**Show**: Command and Query separation from `src/TaskFlow.Application/`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.Application/Commands/CreateTask/CreateTaskCommand.cs
public class CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string ProjectId { get; set; }
    public string? AssigneeId { get; set; }
}

// Reference: taskflow-api-dotnet/src/TaskFlow.Application/Commands/CreateTask/CreateTaskCommandHandler.cs
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate project exists
        var project = await _projectRepository.GetByIdAsync(ProjectId.From(request.ProjectId));
        if (project == null)
            throw new DomainException("Project not found");
            
        // Create task using domain logic
        var task = new Task(
            TaskTitle.From(request.Title),
            TaskDescription.From(request.Description),
            ProjectId.From(request.ProjectId),
            request.AssigneeId != null ? UserId.From(request.AssigneeId) : null
        );
        
        // Save to repository
        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();
        
        // Return DTO
        return new TaskDto
        {
            Id = task.Id.Value,
            Title = task.Title.Value,
            Description = task.Description.Value,
            Status = task.Status.Value,
            ProjectId = task.ProjectId.Value,
            AssigneeId = task.AssigneeId?.Value,
            CreatedAt = task.CreatedAt
        };
    }
}
```

**Key Points**:
- **Separation of Concerns**: Commands for writes, Queries for reads
- **Domain Logic**: Business rules enforced in domain layer
- **Repository Pattern**: Data access abstracted
- **Unit of Work**: Transaction management
- **DTOs**: Data transfer objects for API responses

---

## 🔧 Infrastructure Layer Overview (3 minutes)

### Repository Implementation
**Show**: Task repository implementation from `src/TaskFlow.Infrastructure/Persistence/Repositories/TaskRepository.cs`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.Infrastructure/Persistence/Repositories/TaskRepository.cs
public class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _context;
    
    public TaskRepository(TaskFlowDbContext context)
    {
        _context = context;
    }
    
    public async Task<Task?> GetByIdAsync(TaskId id)
    {
        return await _context.Tasks
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    public async Task<List<Task>> GetAllAsync()
    {
        return await _context.Tasks
            .Include(t => t.Comments)
            .ToListAsync();
    }
    
    public async Task<Task> AddAsync(Task task)
    {
        await _context.Tasks.AddAsync(task);
        return task;
    }
    
    public async Task<Task> UpdateAsync(Task task)
    {
        _context.Tasks.Update(task);
        return task;
    }
}
```

**Key Points**:
- **Interface Implementation**: Implements domain interface
- **EF Core Integration**: Uses Entity Framework for data access
- **Include Statements**: Eager loading of related data
- **Domain Objects**: Returns domain entities, not DTOs

### Database Configuration
**Show**: Entity Framework configuration from `src/TaskFlow.Infrastructure/Persistence/Configurations/`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.Infrastructure/Persistence/Configurations/TaskConfiguration.cs
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeConfigurationBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Title)
            .HasConversion(
                title => title.Value,
                value => TaskTitle.From(value))
            .IsRequired();
            
        builder.Property(t => t.Description)
            .HasConversion(
                description => description.Value,
                value => TaskDescription.From(value))
            .IsRequired();
            
        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Value,
                value => TaskStatus.From(value))
            .IsRequired();
            
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

**Key Points**:
- **Value Object Mapping**: Converts value objects to/from database
- **Relationship Configuration**: Defines foreign key relationships
- **Cascade Rules**: Specifies what happens on delete
- **Domain Integrity**: Ensures database reflects domain rules

---

## 🌐 API Layer Demonstration (2 minutes)

### Controller Implementation
**Show**: Tasks controller from `src/TaskFlow.API/Controllers/TasksController.cs`

```csharp
// Reference: taskflow-api-dotnet/src/TaskFlow.API/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Authorize(Roles = "admin,manager,developer")]
    public async Task<ActionResult<List<TaskDto>>> GetTasks()
    {
        var query = new GetAllTasksQuery();
        var tasks = await _mediator.Send(query);
        return Ok(tasks);
    }
    
    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskCommand command)
    {
        var task = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "admin,manager,developer")]
    public async Task<ActionResult<TaskDto>> GetTask(string id)
    {
        var query = new GetTaskQuery { Id = id };
        var task = await _mediator.Send(query);
        
        if (task == null)
            return NotFound();
            
        return Ok(task);
    }
}
```

**Key Points**:
- **Mediator Pattern**: Uses MediatR for command/query handling
- **Authorization**: Role-based access control
- **HTTP Status Codes**: Proper REST status codes
- **Clean Controllers**: Controllers only handle HTTP concerns

---

## 🧪 Live Demo (5 minutes)

### API Testing with Postman
**Demonstrate**: Live API calls using `taskflow-api-dotnet/postman/TaskFlow-API.postman_collection.json`

#### 1. Health Check
```bash
GET http://localhost:5000/health
```

#### 2. Get All Tasks
```bash
GET http://localhost:5000/api/tasks
Authorization: Bearer <jwt-token>
```

#### 3. Create New Task
```bash
POST http://localhost:5000/api/tasks
Authorization: Bearer <jwt-token>
Content-Type: application/json

{
  "title": "Implement OAuth 2.0",
  "description": "Add OAuth 2.0 authentication to TaskFlow API",
  "projectId": "project-123",
  "assigneeId": "user-456"
}
```

#### 4. Get Specific Task
```bash
GET http://localhost:5000/api/tasks/task-789
Authorization: Bearer <jwt-token>
```

### Key Demo Points:
1. **Domain Validation**: Show validation errors
2. **Business Rules**: Demonstrate task assignment rules
3. **Authorization**: Show role-based access
4. **Error Handling**: Demonstrate proper error responses
5. **Domain Events**: Show events in logs

---

## 🎯 Key Takeaways

### Architecture Benefits:
1. **Separation of Concerns**: Each layer has specific responsibility
2. **Testability**: Easy to unit test domain logic
3. **Maintainability**: Changes isolated to specific layers
4. **Scalability**: Can scale layers independently
5. **Domain Focus**: Business logic drives design

### DDD Benefits:
1. **Business Alignment**: Code reflects business concepts
2. **Ubiquitous Language**: Consistent terminology
3. **Domain Integrity**: Business rules enforced at domain level
4. **Flexibility**: Easy to change implementation details
5. **Collaboration**: Business and technical teams speak same language

---

## 📝 Facilitator Notes

### Demo Flow:
1. **Start with structure**: Show project organization
2. **Highlight domain layer**: Focus on business logic
3. **Show application layer**: Demonstrate use cases
4. **Brief infrastructure**: Show data access
5. **Live API demo**: Prove it works

### Key Questions to Ask:
- "How does this compare to your Day 1 design?"
- "What domain concepts do you see?"
- "How would you add a new feature?"
- "What happens if we change the database?"

### Energy Management:
- **Show enthusiasm**: "Look how clean this is!"
- **Connect concepts**: "This is what we designed yesterday"
- **Build confidence**: "You can build this too!"
- **Encourage questions**: "What's unclear?"

---

## 🚀 Next Steps

### Hands-on Exercise:
"Now let's add a new domain feature together!"

### Day 3 Preview:
"Tomorrow we'll secure this architecture and deploy it to production!" 