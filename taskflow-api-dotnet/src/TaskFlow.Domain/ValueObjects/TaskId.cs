using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.ValueObjects;

public class TaskId : ValueObject
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