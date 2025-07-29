using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.ValueObjects;

public class UserId : ValueObject
{
    public string Value { get; }

    private UserId(string value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid().ToString());
    public static UserId From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 