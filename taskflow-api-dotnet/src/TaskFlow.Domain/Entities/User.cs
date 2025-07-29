using TaskFlow.Domain.Common;
using TaskFlow.Domain.DomainEvents;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class User : AggregateRoot
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Private constructor for EF Core
    private User() 
    {
        Id = default!;
        Email = default!;
        FirstName = default!;
        LastName = default!;
        PasswordHash = default!;
        Role = default!;
        Status = default!;
    }

    public User(Email email, string firstName, string lastName, string passwordHash, UserRole role)
    {
        Id = UserId.New();
        Email = email ?? throw new DomainException("Email cannot be null");
        FirstName = firstName ?? throw new DomainException("First name cannot be null");
        LastName = lastName ?? throw new DomainException("Last name cannot be null");
        PasswordHash = passwordHash ?? throw new DomainException("Password hash cannot be null");
        Role = role ?? throw new DomainException("Role cannot be null");
        Status = UserStatus.Active;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserCreatedEvent(Id.Value, Email.Value, Role.Value));
    }

    // Business Logic Methods
    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be empty");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(Id.Value, FirstName, LastName));
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserPasswordChangedEvent(Id.Value));
    }

    public void ChangeRole(UserRole newRole)
    {
        if (newRole == null)
            throw new DomainException("Role cannot be null");

        var oldRole = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserRoleChangedEvent(Id.Value, oldRole.Value, newRole.Value));
    }

    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            throw new DomainException("User is already inactive");

        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedEvent(Id.Value));
    }

    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new DomainException("User is already active");

        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivatedEvent(Id.Value));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserLoggedInEvent(Id.Value));
    }

    public string GetFullName() => $"{FirstName} {LastName}";
} 