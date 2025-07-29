using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record TaskCreatedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string Title { get; }
    public string ProjectId { get; }
    public string CreatedBy { get; }
    public DateTime OccurredOn { get; }

    public TaskCreatedEvent(string taskId, string title, string projectId, string createdBy)
    {
        TaskId = taskId;
        Title = title;
        ProjectId = projectId;
        CreatedBy = createdBy;
        OccurredOn = DateTime.UtcNow;
    }
} 