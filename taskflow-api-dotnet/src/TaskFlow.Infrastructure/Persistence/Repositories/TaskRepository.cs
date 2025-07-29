using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskFlowDbContext _context;

    public TaskRepository(TaskFlowDbContext context)
    {
        _context = context;
    }

    public async Task<TaskFlow.Domain.Entities.Task?> GetByIdAsync(string id)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id.Value == id);
    }

    public async Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetAllAsync()
    {
        return await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetByProjectIdAsync(string projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId.Value == projectId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetByAssigneeIdAsync(string assigneeId)
    {
        return await _context.Tasks
            .Where(t => t.AssigneeId == assigneeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskFlow.Domain.Entities.Task> AddAsync(TaskFlow.Domain.Entities.Task task)
    {
        var entity = await _context.Tasks.AddAsync(task);
        return entity.Entity;
    }

    public void Update(TaskFlow.Domain.Entities.Task task)
    {
        _context.Tasks.Update(task);
    }

    public void Delete(TaskFlow.Domain.Entities.Task task)
    {
        _context.Tasks.Remove(task);
    }
} 