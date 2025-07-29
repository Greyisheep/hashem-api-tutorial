using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskFlow.API.Models;
using TaskFlow.Application.Commands.CreateTask;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Queries.GetTask;
using TaskFlow.Application.Queries.GetAllTasks;
using MediatR;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Task management operations")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="request">Task creation details</param>
    /// <returns>The created task</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Create a new task",
        Description = "Creates a new task with the specified details",
        OperationId = "CreateTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(CreateTaskCommand request)
    {
        try
        {
            _logger.LogInformation("Creating task: {Title}", request.Title);

            var command = request with { };

            var result = await _mediator.Send(command);

            _logger.LogInformation("Task created successfully: {TaskId}", result.Id);

            var response = ApiResponse<TaskDto>.SuccessResponse(result, "Task created successfully");
            return CreatedAtAction(nameof(GetTask), new { id = result.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task: {Title}", request.Title);
            return StatusCode(500, ApiResponse<TaskDto>.ErrorResponse("INTERNAL_ERROR", "An error occurred while creating the task"));
        }
    }

    /// <summary>
    /// Retrieves a task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task details</returns>
    /// <response code="200">Task found</response>
    /// <response code="404">Task not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get task by ID",
        Description = "Retrieves a task by its unique identifier",
        OperationId = "GetTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving task: {TaskId}", id);

            var query = new GetTaskQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Task not found: {TaskId}", id);
                return NotFound(ApiResponse<TaskDto>.ErrorResponse("TASK_NOT_FOUND", $"Task with ID {id} not found"));
            }

            _logger.LogInformation("Task retrieved successfully: {TaskId}", id);
            return Ok(ApiResponse<TaskDto>.SuccessResponse(result, "Task retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task: {TaskId}", id);
            return StatusCode(500, ApiResponse<TaskDto>.ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving the task"));
        }
    }

    /// <summary>
    /// Gets all tasks
    /// </summary>
    /// <returns>List of all tasks</returns>
    /// <response code="200">Tasks retrieved successfully</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(
        Summary = "Get all tasks",
        Description = "Retrieves all tasks in the system",
        OperationId = "GetAllTasks",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetAllTasks()
    {
        try
        {
            _logger.LogInformation("Retrieving all tasks");

            var query = new GetAllTasksQuery();
            var tasks = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<TaskDto>>.SuccessResponse(tasks, "Tasks retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tasks");
            return StatusCode(500, ApiResponse<IEnumerable<TaskDto>>.ErrorResponse("INTERNAL_ERROR", "An error occurred while retrieving tasks"));
        }
    }
} 