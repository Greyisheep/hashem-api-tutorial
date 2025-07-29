using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class TaskTitle : ValueObject
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