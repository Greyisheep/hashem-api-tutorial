using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Repository interface for Project aggregate root following DDD principles
/// Projects are aggregate roots that encapsulate project management and ownership
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Get all projects (for demo purposes - in production, consider pagination)
    /// </summary>
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project by ID
    /// </summary>
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get projects by owner ID
    /// </summary>
    Task<IEnumerable<Project>> GetByOwnerIdAsync(UserId ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new project
    /// </summary>
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing project
    /// </summary>
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete project by ID
    /// </summary>
    Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default);
} 