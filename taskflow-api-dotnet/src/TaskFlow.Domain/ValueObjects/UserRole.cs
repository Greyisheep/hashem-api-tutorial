using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.ValueObjects;

public class UserRole : ValueObject
{
    public string Value { get; }

    private UserRole(string value)
    {
        if (!IsValid(value))
            throw new DomainException($"Invalid user role: {value}");

        Value = value;
    }

    public static UserRole Admin => new("Admin");
    public static UserRole ProjectManager => new("ProjectManager");
    public static UserRole Developer => new("Developer");
    public static UserRole Viewer => new("Viewer");

    public static UserRole From(string value) => new(value);

    private static bool IsValid(string value)
    {
        return value switch
        {
            "Admin" => true,
            "ProjectManager" => true,
            "Developer" => true,
            "Viewer" => true,
            _ => false
        };
    }

    public bool CanManageUsers() => Value == "Admin";
    public bool CanManageProjects() => Value is "Admin" or "ProjectManager";
    public bool CanEditTasks() => Value is "Admin" or "ProjectManager" or "Developer";
    public bool CanViewTasks() => true; // All roles can view tasks

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 