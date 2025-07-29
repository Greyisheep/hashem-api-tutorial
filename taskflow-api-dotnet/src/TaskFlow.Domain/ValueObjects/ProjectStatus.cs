using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class ProjectStatus : ValueObject
{
    public string Value { get; }

    private ProjectStatus(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid project status: {value}");

        Value = value;
    }

    public static ProjectStatus Planning => new("Planning");
    public static ProjectStatus Active => new("Active");
    public static ProjectStatus Completed => new("Completed");
    public static ProjectStatus Cancelled => new("Cancelled");

    public static ProjectStatus From(string value) => new(value);

    private static bool IsValid(string value)
    {
        return value switch
        {
            "Planning" => true,
            "Active" => true,
            "Completed" => true,
            "Cancelled" => true,
            _ => false
        };
    }

    public bool CanTransitionTo(ProjectStatus newStatus)
    {
        return Value switch
        {
            "Planning" => newStatus.Value is "Active" or "Cancelled",
            "Active" => newStatus.Value is "Completed" or "Cancelled",
            "Completed" => false, // Cannot transition from completed
            "Cancelled" => false, // Cannot transition from cancelled
            _ => false
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 