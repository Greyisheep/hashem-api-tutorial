using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class UserStatus : ValueObject
{
    public string Value { get; }

    private UserStatus(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid user status: {value}");

        Value = value;
    }

    public static UserStatus Active => new("Active");
    public static UserStatus Inactive => new("Inactive");
    public static UserStatus Suspended => new("Suspended");

    public static UserStatus From(string value) => new(value);

    private static bool IsValid(string value)
    {
        return value switch
        {
            "Active" => true,
            "Inactive" => true,
            "Suspended" => true,
            _ => false
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 