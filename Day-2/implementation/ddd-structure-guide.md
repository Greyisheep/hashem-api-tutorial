# Domain-Driven Design Implementation Guide - .NET

## 🎯 Implementation Objectives
- Structure .NET APIs following DDD principles
- Implement rich domain models vs anemic models
- Create clean architecture with proper separation of concerns
- Build production-ready .NET TaskFlow API

## ⏰ Implementation Timing: 60 minutes

---

## 🏗️ Project Structure Overview

### Clean Architecture with DDD
```
TaskFlow.API/
├── TaskFlow.Domain/           # Business logic, entities, value objects
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Aggregates/
│   ├── DomainEvents/
│   └── Interfaces/
├── TaskFlow.Application/      # Use cases, DTOs, interfaces
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
├── TaskFlow.Infrastructure/   # Data access, external services
│   ├── Persistence/
│   ├── ExternalServices/
│   ├── Messaging/
│   └── Configuration/
├── TaskFlow.API/             # Controllers, middleware, configuration
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   └── Configuration/
└── TaskFlow.Tests/           # Unit and integration tests
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── API/
```

---

## 🏛️ Domain Layer Implementation

### Step 1: Domain Entities

#### Task Entity (Rich Domain Model)
```csharp
// TaskFlow.Domain/Entities/Task.cs
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.DomainEvents;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

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
    public Priority Priority { get; private set; }
    public List<Comment> Comments { get; private set; } = new();

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
        Priority = Priority.Medium;
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

    public void UpdateStatus(TaskStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw new DomainException($"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskStatusChangedEvent(Id, Status.Value));
    }

    public void AddComment(Comment comment)
    {
        Comments.Add(comment ?? throw new DomainException("Comment cannot be null"));
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CommentAddedEvent(Id, comment.Id.Value));
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority ?? throw new DomainException("Priority cannot be null");
        UpdatedAt = DateTime.UtcNow;
    }

    private bool CanTransitionTo(TaskStatus newStatus)
    {
        return Status switch
        {
            TaskStatus.Pending => newStatus == TaskStatus.InProgress || newStatus == TaskStatus.Cancelled,
            TaskStatus.InProgress => newStatus == TaskStatus.Completed || newStatus == TaskStatus.Cancelled,
            TaskStatus.Completed => false, // Cannot change from completed
            TaskStatus.Cancelled => false, // Cannot change from cancelled
            _ => false
        };
    }
}
```

#### Project Entity
```csharp
// TaskFlow.Domain/Entities/Project.cs
namespace TaskFlow.Domain.Entities;

public class Project : AggregateRoot
{
    public ProjectId Id { get; private set; }
    public ProjectName Name { get; private set; }
    public ProjectDescription Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public List<UserId> TeamMembers { get; private set; } = new();
    public List<Task> Tasks { get; private set; } = new();

    private Project() { }

    public Project(ProjectName name, ProjectDescription description, UserId ownerId)
    {
        Id = ProjectId.New();
        Name = name ?? throw new DomainException("Project name cannot be null");
        Description = description ?? throw new DomainException("Project description cannot be null");
        OwnerId = ownerId ?? throw new DomainException("Owner ID cannot be null");
        Status = ProjectStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddTeamMember(ownerId); // Owner is automatically a team member
        AddDomainEvent(new ProjectCreatedEvent(Id, Name.Value, OwnerId.Value));
    }

    public void AddTeamMember(UserId userId)
    {
        if (TeamMembers.Contains(userId))
            throw new DomainException("User is already a team member");

        TeamMembers.Add(userId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TeamMemberAddedEvent(Id, userId.Value));
    }

    public void RemoveTeamMember(UserId userId)
    {
        if (userId == OwnerId)
            throw new DomainException("Cannot remove project owner");

        if (!TeamMembers.Contains(userId))
            throw new DomainException("User is not a team member");

        TeamMembers.Remove(userId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TeamMemberRemovedEvent(Id, userId.Value));
    }

    public void AddTask(Task task)
    {
        if (task.ProjectId != Id)
            throw new DomainException("Task does not belong to this project");

        Tasks.Add(task);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ProjectStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectStatusChangedEvent(Id, Status.Value));
    }
}
```

### Step 2: Value Objects

#### TaskId Value Object
```csharp
// TaskFlow.Domain/ValueObjects/TaskId.cs
namespace TaskFlow.Domain.ValueObjects;

public record TaskId : ValueObject
{
    public string Value { get; }

    private TaskId(string value)
    {
        Value = value;
    }

    public static TaskId New() => new(Guid.NewGuid().ToString());
    public static TaskId From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

#### TaskTitle Value Object
```csharp
// TaskFlow.Domain/ValueObjects/TaskTitle.cs
namespace TaskFlow.Domain.ValueObjects;

public record TaskTitle : ValueObject
{
    public string Value { get; }

    private TaskTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Task title cannot be empty");

        if (value.Length > 200)
            throw new DomainException("Task title cannot exceed 200 characters");

        Value = value.Trim();
    }

    public static TaskTitle From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

#### TaskStatus Value Object
```csharp
// TaskFlow.Domain/ValueObjects/TaskStatus.cs
namespace TaskFlow.Domain.ValueObjects;

public record TaskStatus : ValueObject
{
    public string Value { get; }

    private TaskStatus(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid task status: {value}");

        Value = value;
    }

    public static TaskStatus Pending => new("pending");
    public static TaskStatus InProgress => new("in_progress");
    public static TaskStatus Completed => new("completed");
    public static TaskStatus Cancelled => new("cancelled");

    private static bool IsValid(string value)
    {
        return value switch
        {
            "pending" => true,
            "in_progress" => true,
            "completed" => true,
            "cancelled" => true,
            _ => false
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

### Step 3: Domain Events

#### TaskCreatedEvent
```csharp
// TaskFlow.Domain/DomainEvents/TaskCreatedEvent.cs
namespace TaskFlow.Domain.DomainEvents;

public record TaskCreatedEvent : IDomainEvent
{
    public TaskId TaskId { get; }
    public string Title { get; }
    public string ProjectId { get; }
    public DateTime OccurredOn { get; }

    public TaskCreatedEvent(TaskId taskId, string title, string projectId)
    {
        TaskId = taskId;
        Title = title;
        ProjectId = projectId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

### Step 4: Domain Services

#### TaskAssignmentService
```csharp
// TaskFlow.Domain/Services/TaskAssignmentService.cs
namespace TaskFlow.Domain.Services;

public interface ITaskAssignmentService
{
    Task<bool> CanAssignTaskToUser(TaskId taskId, UserId userId);
    Task<UserId> SuggestAssignee(TaskId taskId);
}

public class TaskAssignmentService : ITaskAssignmentService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskAssignmentService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> CanAssignTaskToUser(TaskId taskId, UserId userId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        var user = await _userRepository.GetByIdAsync(userId);

        if (task == null || user == null)
            return false;

        // Business rule: User must be a team member of the project
        return task.ProjectId != null && 
               await _userRepository.IsTeamMemberAsync(userId, task.ProjectId);
    }

    public async Task<UserId> SuggestAssignee(TaskId taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new DomainException("Task not found");

        // Business rule: Suggest user with least workload
        var availableUsers = await _userRepository.GetTeamMembersAsync(task.ProjectId);
        var userWorkloads = await _taskRepository.GetUserWorkloadsAsync(availableUsers.Select(u => u.Id));

        var suggestedUser = userWorkloads
            .OrderBy(w => w.TaskCount)
            .FirstOrDefault();

        return suggestedUser?.UserId ?? throw new DomainException("No available assignees");
    }
}
```

---

## 📋 Application Layer Implementation

### Step 5: Commands and Queries

#### CreateTaskCommand
```csharp
// TaskFlow.Application/Commands/CreateTask/CreateTaskCommand.cs
namespace TaskFlow.Application.Commands.CreateTask;

public record CreateTaskCommand : IRequest<CreateTaskResponse>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProjectId { get; init; } = string.Empty;
    public string? AssigneeId { get; init; }
    public string Priority { get; init; } = "medium";
}

public record CreateTaskResponse
{
    public string TaskId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, CreateTaskResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<CreateTaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate project exists
        var projectId = ProjectId.From(request.ProjectId);
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            throw new ApplicationException($"Project with ID {request.ProjectId} not found");

        // Create task
        var task = new Task(
            TaskTitle.From(request.Title),
            TaskDescription.From(request.Description),
            projectId,
            request.AssigneeId != null ? UserId.From(request.AssigneeId) : null
        );

        if (request.Priority != "medium")
        {
            task.SetPriority(Priority.From(request.Priority));
        }

        // Save to repository
        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        await _eventDispatcher.DispatchEventsAsync(task.DomainEvents);

        return new CreateTaskResponse
        {
            TaskId = task.Id.Value,
            Title = task.Title.Value,
            Status = task.Status.Value,
            CreatedAt = task.CreatedAt
        };
    }
}
```

#### GetTaskQuery
```csharp
// TaskFlow.Application/Queries/GetTask/GetTaskQuery.cs
namespace TaskFlow.Application.Queries.GetTask;

public record GetTaskQuery : IRequest<GetTaskResponse?>
{
    public string TaskId { get; init; } = string.Empty;
}

public record GetTaskResponse
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? AssigneeId { get; init; }
    public string ProjectId { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<CommentDto> Comments { get; init; } = new();
}

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, GetTaskResponse?>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<GetTaskResponse?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        var taskId = TaskId.From(request.TaskId);
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task == null)
            return null;

        return new GetTaskResponse
        {
            Id = task.Id.Value,
            Title = task.Title.Value,
            Description = task.Description.Value,
            Status = task.Status.Value,
            AssigneeId = task.AssigneeId?.Value,
            ProjectId = task.ProjectId.Value,
            Priority = task.Priority.Value,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Comments = task.Comments.Select(c => new CommentDto
            {
                Id = c.Id.Value,
                Content = c.Content.Value,
                AuthorId = c.AuthorId.Value,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }
}
```

### Step 6: DTOs

#### CommentDto
```csharp
// TaskFlow.Application/DTOs/CommentDto.cs
namespace TaskFlow.Application.DTOs;

public record CommentDto
{
    public string Id { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
```

---

## 🗄️ Infrastructure Layer Implementation

### Step 7: Repository Implementation

#### TaskRepository
```csharp
// TaskFlow.Infrastructure/Persistence/Repositories/TaskRepository.cs
namespace TaskFlow.Infrastructure.Persistence.Repositories;

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

    public async Task<IEnumerable<Task>> GetByProjectIdAsync(ProjectId projectId)
    {
        return await _context.Tasks
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task AddAsync(Task task)
    {
        await _context.Tasks.AddAsync(task);
    }

    public void Update(Task task)
    {
        _context.Tasks.Update(task);
    }

    public void Delete(Task task)
    {
        _context.Tasks.Remove(task);
    }

    public async Task<IEnumerable<UserWorkload>> GetUserWorkloadsAsync(IEnumerable<UserId> userIds)
    {
        return await _context.Tasks
            .Where(t => t.AssigneeId != null && userIds.Contains(t.AssigneeId))
            .GroupBy(t => t.AssigneeId)
            .Select(g => new UserWorkload(g.Key!, g.Count()))
            .ToListAsync();
    }
}
```

### Step 8: Entity Framework Configuration

#### TaskConfiguration
```csharp
// TaskFlow.Infrastructure/Persistence/Configurations/TaskConfiguration.cs
namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TaskId.From(value))
            .IsRequired();

        builder.Property(t => t.Title)
            .HasConversion(
                title => title.Value,
                value => TaskTitle.From(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasConversion(
                desc => desc.Value,
                value => TaskDescription.From(value))
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Value,
                value => TaskStatus.From(value))
            .IsRequired();

        builder.Property(t => t.AssigneeId)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value));

        builder.Property(t => t.ProjectId)
            .HasConversion(
                id => id.Value,
                value => ProjectId.From(value))
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion(
                priority => priority.Value,
                value => Priority.From(value))
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(t => t.Comments)
            .WithOne()
            .HasForeignKey("TaskId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.Status);
    }
}
```

---

## 🌐 API Layer Implementation

### Step 9: Controllers

#### TasksController
```csharp
// TaskFlow.API/Controllers/TasksController.cs
namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateTaskResponse>> CreateTask(CreateTaskCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTask), new { id = result.TaskId }, result);
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error creating task");
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error creating task");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetTaskResponse>> GetTask(string id)
    {
        var query = new GetTaskQuery { TaskId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}/assign")]
    public async Task<ActionResult> AssignTask(string id, AssignTaskCommand command)
    {
        command.TaskId = id;
        
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error assigning task");
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error assigning task");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateTaskStatus(string id, UpdateTaskStatusCommand command)
    {
        command.TaskId = id;
        
        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning(ex, "Application error updating task status");
            return BadRequest(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error updating task status");
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

### Step 10: Program.cs Configuration

#### Program.cs
```csharp
// TaskFlow.API/Program.cs
using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
    context.Database.EnsureCreated();
}

app.Run();
```

---

## 🧪 Testing Implementation

### Step 11: Unit Tests

#### TaskEntityTests
```csharp
// TaskFlow.Tests/Domain/TaskTests.cs
namespace TaskFlow.Tests.Domain;

public class TaskTests
{
    [Fact]
    public void CreateTask_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var title = TaskTitle.From("Test Task");
        var description = TaskDescription.From("Test Description");
        var projectId = ProjectId.New();

        // Act
        var task = new Task(title, description, projectId);

        // Assert
        task.Title.Should().Be(title);
        task.Description.Should().Be(description);
        task.ProjectId.Should().Be(projectId);
        task.Status.Should().Be(TaskStatus.Pending);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AssignTask_ToValidUser_ShouldUpdateAssigneeAndStatus()
    {
        // Arrange
        var task = CreateTestTask();
        var assigneeId = UserId.New();

        // Act
        task.AssignTo(assigneeId);

        // Assert
        task.AssigneeId.Should().Be(assigneeId);
        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public void AssignTask_ToCompletedTask_ShouldThrowException()
    {
        // Arrange
        var task = CreateTestTask();
        task.UpdateStatus(TaskStatus.Completed);
        var assigneeId = UserId.New();

        // Act & Assert
        var action = () => task.AssignTo(assigneeId);
        action.Should().Throw<DomainException>()
              .WithMessage("Cannot assign completed tasks");
    }

    private static Task CreateTestTask()
    {
        return new Task(
            TaskTitle.From("Test Task"),
            TaskDescription.From("Test Description"),
            ProjectId.New()
        );
    }
}
```

---

## 🎯 Key Learning Points

### DDD Benefits
1. **Rich Domain Models**: Business logic encapsulated in entities
2. **Type Safety**: Value objects prevent invalid states
3. **Domain Events**: Loose coupling between aggregates
4. **Clean Architecture**: Clear separation of concerns
5. **Testability**: Easy to unit test business logic

### Best Practices
1. **Keep Entities Focused**: Single responsibility principle
2. **Use Value Objects**: For immutable concepts
3. **Domain Events**: For cross-aggregate communication
4. **Repository Pattern**: Abstract data access
5. **CQRS**: Separate read and write models when needed

### Common Pitfalls
1. **Anemic Models**: Don't put business logic in services
2. **Over-Engineering**: Start simple, add complexity when needed
3. **Tight Coupling**: Use interfaces and dependency injection
4. **Data-Driven Design**: Focus on business concepts, not database structure

---

**Implementation Success**: Students can structure .NET APIs following DDD principles and implement production-ready domain models with proper separation of concerns. 