namespace TaskFlow.Application.DTOs;

public record TaskDto
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? AssigneeId { get; init; }
    public string ProjectId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}



public record UpdateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record AssignTaskRequest
{
    public string AssigneeId { get; init; } = string.Empty;
}

public record UpdateTaskStatusRequest
{
    public string Status { get; init; } = string.Empty;
} 