using Microsoft.AspNetCore.Identity;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<TaskFlow.Domain.Entities.Task> AssignedTasks { get; set; } = new List<TaskFlow.Domain.Entities.Task>();
    public virtual ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
} 