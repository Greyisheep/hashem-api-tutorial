using TaskFlow.Domain.Common;
using TaskFlow.Domain.DomainEvents;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class Task : AggregateRoot
{
    public TaskId Id { get; private set; }
    public TaskTitle Title { get; private set; }
    public string Description { get; private set; }
    public TaskState Status { get; private set; }
    public string? AssigneeId { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; }

    // Private constructor for EF Core
    private Task() 
    {
        Id = default!;
        Title = default!;
        Description = default!;
        Status = default!;
        ProjectId = default!;
        CreatedBy = default!;
    }

    public Task(TaskTitle title, string description, ProjectId projectId, string createdBy)
    {
        Id = TaskId.New();
        Title = title ?? throw new DomainException("Task title cannot be null");
        Description = description ?? throw new DomainException("Task description cannot be null");
        ProjectId = projectId ?? throw new DomainException("Project ID cannot be null");
        CreatedBy = createdBy ?? throw new DomainException("Created by cannot be null");
        Status = TaskState.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskCreatedEvent(Id.Value, Title.Value, ProjectId.Value, CreatedBy));
    }

    // Business Logic Methods
    public void AssignTo(string assigneeId)
    {
        if (string.IsNullOrWhiteSpace(assigneeId))
            throw new DomainException("Assignee ID cannot be empty");

        AssigneeId = assigneeId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskAssignedEvent(Id.Value, assigneeId));
    }

    public void UpdateStatus(TaskState newStatus)
    {
        if (newStatus == null)
            throw new DomainException("Status cannot be null");

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskStatusChangedEvent(Id.Value, newStatus.Value));
    }

    public void UpdateDetails(TaskTitle title, string description)
    {
        Title = title ?? throw new DomainException("Task title cannot be null");
        Description = description ?? throw new DomainException("Task description cannot be null");
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskUpdatedEvent(Id.Value, Title.Value));
    }
} 