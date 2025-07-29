using MediatR;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace TaskFlow.Application.Commands.CreateTask;

public record CreateTaskCommand : IRequest<TaskDto>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProjectId { get; init; } = string.Empty;
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating task: {Title} for project: {ProjectId}",
            request.Title, request.ProjectId);

        var task = new TaskFlow.Domain.Entities.Task(
            TaskTitle.From(request.Title),
            request.Description,
            ProjectId.From(request.ProjectId),
            "system" // TODO: Get from current user context
        );

        var createdTask = await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task created successfully with ID: {TaskId}", createdTask.Id.Value);

        return new TaskDto
        {
            Id = createdTask.Id.Value,
            Title = createdTask.Title.Value,
            Description = createdTask.Description,
            Status = createdTask.Status.Value,
            AssigneeId = createdTask.AssigneeId,
            ProjectId = createdTask.ProjectId.Value,
            CreatedAt = createdTask.CreatedAt,
            UpdatedAt = createdTask.UpdatedAt,
            CreatedBy = createdTask.CreatedBy
        };
    }
} 