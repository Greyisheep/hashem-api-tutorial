using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Models;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Projects Controller - Following DDD principles for Project aggregate root management
/// Handles HTTP requests for project-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectRepository projectRepository, ILogger<ProjectsController> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects - Following DDD: Returns domain entities
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Project>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Project>>>> GetAllProjects()
    {
        try
        {
            _logger.LogInformation("Getting all projects");
            var projects = await _projectRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Project>>.SuccessResponse(projects, "Projects retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, ApiResponse<IEnumerable<Project>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get project by ID - Following DDD: Uses value objects for identity
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Project>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Project>), 404)]
    public async Task<ActionResult<ApiResponse<Project>>> GetProject(string id)
    {
        try
        {
            _logger.LogInformation("Getting project with ID: {ProjectId}", id);
            var projectId = ProjectId.From(id);
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null)
            {
                return NotFound(ApiResponse<Project>.ErrorResponse("Project not found"));
            }

            return Ok(ApiResponse<Project>.SuccessResponse(project, "Project retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project with ID: {ProjectId}", id);
            return StatusCode(500, ApiResponse<Project>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get projects by owner - Following DDD: Uses value objects for business rules
    /// </summary>
    [HttpGet("owner/{ownerId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Project>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Project>>>> GetProjectsByOwner(string ownerId)
    {
        try
        {
            _logger.LogInformation("Getting projects for owner: {OwnerId}", ownerId);
            var userId = UserId.From(ownerId);
            var projects = await _projectRepository.GetByOwnerIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<Project>>.SuccessResponse(projects, "Projects retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects for owner: {OwnerId}", ownerId);
            return StatusCode(500, ApiResponse<IEnumerable<Project>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Create new project - Following DDD: Ensures aggregate consistency
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Project>), 201)]
    [ProducesResponseType(typeof(ApiResponse<Project>), 400)]
    public async Task<ActionResult<ApiResponse<Project>>> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new project: {ProjectName}", request.Name);
            
            // Following DDD: Create domain entity with proper validation
            var projectName = ProjectName.From(request.Name);
            var ownerId = UserId.From(request.OwnerId);
            var project = new Project(
                projectName,
                request.Description,
                ownerId
            );

            var createdProject = await _projectRepository.AddAsync(project);
            return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id.Value }, 
                ApiResponse<Project>.SuccessResponse(createdProject, "Project created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, ApiResponse<Project>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Update project - Following DDD: Ensures aggregate consistency
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Project>), 200)]
    [ProducesResponseType(typeof(ApiResponse<Project>), 404)]
    public async Task<ActionResult<ApiResponse<Project>>> UpdateProject(string id, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Updating project with ID: {ProjectId}", id);
            var projectId = ProjectId.From(id);
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null)
            {
                return NotFound(ApiResponse<Project>.ErrorResponse("Project not found"));
            }

            // Following DDD: Update domain entity while maintaining consistency
            var projectName = ProjectName.From(request.Name);
            project.UpdateDetails(projectName, request.Description);

            var updatedProject = await _projectRepository.UpdateAsync(project);
            return Ok(ApiResponse<Project>.SuccessResponse(updatedProject, "Project updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project with ID: {ProjectId}", id);
            return StatusCode(500, ApiResponse<Project>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Delete project - Following DDD: Handles aggregate deletion
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(string id)
    {
        try
        {
            _logger.LogInformation("Deleting project with ID: {ProjectId}", id);
            var projectId = ProjectId.From(id);
            var success = await _projectRepository.DeleteAsync(projectId);
            
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Project not found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Project deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project with ID: {ProjectId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error"));
        }
    }
}

/// <summary>
/// DTOs for project operations - Following DDD: Keep DTOs simple and focused
/// </summary>
public record CreateProjectRequest(string Name, string Description, string OwnerId, string? Status);
public record UpdateProjectRequest(string Name, string Description, string? Status); 