using TaskFlow.Domain.Common;
using TaskFlow.Domain.DomainEvents;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Domain.Entities;

public class Project : AggregateRoot
{
    public ProjectId Id { get; private set; }
    public ProjectName Name { get; private set; }
    public string Description { get; private set; }
    public ProjectStatus Status { get; private set; }
    public UserId OwnerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    private readonly List<UserId> _members = new();
    public IReadOnlyList<UserId> Members => _members.AsReadOnly();

    // Private constructor for EF Core
    private Project() 
    {
        Id = default!;
        Name = default!;
        Description = default!;
        Status = default!;
        OwnerId = default!;
    }

    public Project(ProjectName name, string description, UserId ownerId)
    {
        Id = ProjectId.New();
        Name = name ?? throw new DomainException("Project name cannot be null");
        Description = description ?? throw new DomainException("Project description cannot be null");
        OwnerId = ownerId ?? throw new DomainException("Owner ID cannot be null");
        Status = ProjectStatus.Planning;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Owner is automatically a member
        _members.Add(ownerId);

        AddDomainEvent(new ProjectCreatedEvent(Id.Value, Name.Value, OwnerId.Value));
    }

    // Business Logic Methods
    public void UpdateDetails(ProjectName name, string description)
    {
        Name = name ?? throw new DomainException("Project name cannot be null");
        Description = description ?? throw new DomainException("Project description cannot be null");
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectUpdatedEvent(Id.Value, Name.Value));
    }

    public void SetDates(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new DomainException("Start date cannot be after end date");

        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectDatesUpdatedEvent(Id.Value, StartDate, EndDate));
    }

    public void AddMember(UserId userId)
    {
        if (userId == null)
            throw new DomainException("User ID cannot be null");

        if (_members.Contains(userId))
            throw new DomainException("User is already a member of this project");

        _members.Add(userId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectMemberAddedEvent(Id.Value, userId.Value));
    }

    public void RemoveMember(UserId userId)
    {
        if (userId == null)
            throw new DomainException("User ID cannot be null");

        if (userId == OwnerId)
            throw new DomainException("Cannot remove project owner from members");

        if (!_members.Contains(userId))
            throw new DomainException("User is not a member of this project");

        _members.Remove(userId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectMemberRemovedEvent(Id.Value, userId.Value));
    }

    public void Start()
    {
        if (Status != ProjectStatus.Planning)
            throw new DomainException("Only projects in planning can be started");

        Status = ProjectStatus.Active;
        StartDate ??= DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectStartedEvent(Id.Value));
    }

    public void Complete()
    {
        if (Status != ProjectStatus.Active)
            throw new DomainException("Only active projects can be completed");

        Status = ProjectStatus.Completed;
        EndDate ??= DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCompletedEvent(Id.Value));
    }

    public void Cancel()
    {
        if (Status == ProjectStatus.Completed)
            throw new DomainException("Cannot cancel completed projects");

        Status = ProjectStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCancelledEvent(Id.Value));
    }

    public bool IsMember(UserId userId) => _members.Contains(userId);
    public bool IsOwner(UserId userId) => OwnerId == userId;
} 