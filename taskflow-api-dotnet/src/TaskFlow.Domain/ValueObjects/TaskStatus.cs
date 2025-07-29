using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class TaskState : ValueObject
{
    public string Value { get; }

    private TaskState(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid task status: {value}");

        Value = value;
    }

    public static TaskState Pending => new("pending");
    public static TaskState InProgress => new("in_progress");
    public static TaskState Completed => new("completed");
    public static TaskState Cancelled => new("cancelled");

    public static TaskState From(string value) => new(value);

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