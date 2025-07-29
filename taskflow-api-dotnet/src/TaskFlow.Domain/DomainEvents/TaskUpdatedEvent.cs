using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record TaskUpdatedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }

    public TaskUpdatedEvent(string taskId, string title)
    {
        TaskId = taskId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
} 