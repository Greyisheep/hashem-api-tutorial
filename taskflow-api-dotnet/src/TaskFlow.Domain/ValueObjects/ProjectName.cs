using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class ProjectName : ValueObject
{
    public string Value { get; }

    private ProjectName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Project name cannot be empty");

        if (value.Length > 200)
            throw new DomainException("Project name cannot exceed 200 characters");

        Value = value.Trim();
    }

    public static ProjectName From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 