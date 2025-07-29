using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Application.Queries.GetAllTasks;

public record GetAllTasksQuery : IRequest<IEnumerable<TaskDto>>;

public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, IEnumerable<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetAllTasksQueryHandler> _logger;

    public GetAllTasksQueryHandler(ITaskRepository taskRepository, ILogger<GetAllTasksQueryHandler> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all tasks");

        var tasks = await _taskRepository.GetAllAsync();

        _logger.LogInformation("Retrieved {TaskCount} tasks", tasks.Count());

        return tasks.Select(task => new TaskDto
        {
            Id = task.Id.Value,
            Title = task.Title.Value,
            Description = task.Description,
            Status = task.Status.Value,
            AssigneeId = task.AssigneeId,
            ProjectId = task.ProjectId.Value,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CreatedBy = task.CreatedBy
        });
    }
} 