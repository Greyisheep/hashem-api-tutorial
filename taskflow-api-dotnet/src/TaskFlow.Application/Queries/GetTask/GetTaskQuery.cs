using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Application.Queries.GetTask;

public record GetTaskQuery(string Id) : IRequest<TaskDto?>;

public class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, TaskDto?>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetTaskQueryHandler> _logger;

    public GetTaskQueryHandler(ITaskRepository taskRepository, ILogger<GetTaskQueryHandler> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<TaskDto?> Handle(GetTaskQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting task with ID: {TaskId}", request.Id);

        var task = await _taskRepository.GetByIdAsync(request.Id);

        if (task == null)
        {
            _logger.LogWarning("Task not found with ID: {TaskId}", request.Id);
            return null;
        }

        _logger.LogInformation("Task retrieved successfully: {TaskId}", task.Id.Value);

        return new TaskDto
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
        };
    }
} 