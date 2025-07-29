using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record ProjectCreatedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public string Name { get; }
    public string OwnerId { get; }
    public DateTime OccurredOn { get; }

    public ProjectCreatedEvent(string projectId, string name, string ownerId)
    {
        ProjectId = projectId;
        Name = name;
        OwnerId = ownerId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectUpdatedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public string Name { get; }
    public DateTime OccurredOn { get; }

    public ProjectUpdatedEvent(string projectId, string name)
    {
        ProjectId = projectId;
        Name = name;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectDatesUpdatedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public DateTime OccurredOn { get; }

    public ProjectDatesUpdatedEvent(string projectId, DateTime? startDate, DateTime? endDate)
    {
        ProjectId = projectId;
        StartDate = startDate;
        EndDate = endDate;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectMemberAddedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public ProjectMemberAddedEvent(string projectId, string userId)
    {
        ProjectId = projectId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectMemberRemovedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public ProjectMemberRemovedEvent(string projectId, string userId)
    {
        ProjectId = projectId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectStartedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public DateTime OccurredOn { get; }

    public ProjectStartedEvent(string projectId)
    {
        ProjectId = projectId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectCompletedEvent : IDomainEvent
{
    public string ProjectId { get; }
    public DateTime OccurredOn { get; }

    public ProjectCompletedEvent(string projectId)
    {
        ProjectId = projectId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record ProjectCancelledEvent : IDomainEvent
{
    public string ProjectId { get; }
    public DateTime OccurredOn { get; }

    public ProjectCancelledEvent(string projectId)
    {
        ProjectId = projectId;
        OccurredOn = DateTime.UtcNow;
    }
} 