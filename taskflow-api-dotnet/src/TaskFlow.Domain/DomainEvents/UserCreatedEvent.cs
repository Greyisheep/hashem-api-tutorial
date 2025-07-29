using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.DomainEvents;

public record UserCreatedEvent : IDomainEvent
{
    public string UserId { get; }
    public string Email { get; }
    public string Role { get; }
    public DateTime OccurredOn { get; }

    public UserCreatedEvent(string userId, string email, string role)
    {
        UserId = userId;
        Email = email;
        Role = role;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserProfileUpdatedEvent : IDomainEvent
{
    public string UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime OccurredOn { get; }

    public UserProfileUpdatedEvent(string userId, string firstName, string lastName)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserPasswordChangedEvent : IDomainEvent
{
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public UserPasswordChangedEvent(string userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserRoleChangedEvent : IDomainEvent
{
    public string UserId { get; }
    public string OldRole { get; }
    public string NewRole { get; }
    public DateTime OccurredOn { get; }

    public UserRoleChangedEvent(string userId, string oldRole, string newRole)
    {
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserDeactivatedEvent : IDomainEvent
{
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public UserDeactivatedEvent(string userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserActivatedEvent : IDomainEvent
{
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public UserActivatedEvent(string userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

public record UserLoggedInEvent : IDomainEvent
{
    public string UserId { get; }
    public DateTime OccurredOn { get; }

    public UserLoggedInEvent(string userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
} 