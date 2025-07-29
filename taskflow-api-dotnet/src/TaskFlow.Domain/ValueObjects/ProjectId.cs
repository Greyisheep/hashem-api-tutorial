using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.ValueObjects;

public class ProjectId : ValueObject
{
    public string Value { get; }

    private ProjectId(string value)
    {
        Value = value;
    }

    public static ProjectId New() => new(Guid.NewGuid().ToString());
    public static ProjectId From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 