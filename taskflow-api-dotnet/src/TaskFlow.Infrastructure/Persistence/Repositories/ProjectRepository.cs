using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Project repository implementation following DDD principles
/// Handles persistence of Project aggregate root using Entity Framework Core
/// </summary>
public class ProjectRepository : IProjectRepository
{
    private readonly TaskFlowDbContext _context;

    public ProjectRepository(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Following DDD: Return domain entities, not DTOs
        return await _context.Projects
            .AsNoTracking() // Read-only for performance
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        // Following DDD: Use value objects for identity
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByOwnerIdAsync(UserId ownerId, CancellationToken cancellationToken = default)
    {
        // Following DDD: Use value objects for business rules
        return await _context.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        // Following DDD: Ensure aggregate consistency
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        // Following DDD: Ensure aggregate consistency
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<bool> DeleteAsync(ProjectId id, CancellationToken cancellationToken = default)
    {
        // Following DDD: Handle aggregate deletion
        var project = await GetByIdAsync(id, cancellationToken);
        if (project == null)
            return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
} 