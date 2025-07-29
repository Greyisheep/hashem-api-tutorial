using TaskFlow.Domain.Common;
using TaskFlow.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace TaskFlow.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");

        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");

        Value = value.ToLowerInvariant().Trim();
    }

    public static Email From(string value) => new(value);

    private static bool IsValidEmail(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
} 