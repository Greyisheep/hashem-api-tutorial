using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskFlow.Domain.Entities.Task?> GetByIdAsync(string id);
    Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetAllAsync();
    Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetByProjectIdAsync(string projectId);
    Task<IEnumerable<TaskFlow.Domain.Entities.Task>> GetByAssigneeIdAsync(string assigneeId);
    Task<TaskFlow.Domain.Entities.Task> AddAsync(TaskFlow.Domain.Entities.Task task);
    void Update(TaskFlow.Domain.Entities.Task task);
    void Delete(TaskFlow.Domain.Entities.Task task);
} 