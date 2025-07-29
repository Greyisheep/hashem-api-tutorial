using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record TaskStatusChangedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string Status { get; }
    public DateTime OccurredOn { get; }

    public TaskStatusChangedEvent(string taskId, string status)
    {
        TaskId = taskId;
        Status = status;
        OccurredOn = DateTime.UtcNow;
    }
} 