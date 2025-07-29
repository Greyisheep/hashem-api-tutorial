using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record TaskAssignedEvent : IDomainEvent
{
    public string TaskId { get; }
    public string AssigneeId { get; }
    public DateTime OccurredOn { get; }

    public TaskAssignedEvent(string taskId, string assigneeId)
    {
        TaskId = taskId;
        AssigneeId = assigneeId;
        OccurredOn = DateTime.UtcNow;
    }
} 