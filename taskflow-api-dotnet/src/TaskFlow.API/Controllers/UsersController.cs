using Microsoft.AspNetCore.Mvc;
using TaskFlow.API.Models;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Users Controller - Following DDD principles for User aggregate root management
/// Handles HTTP requests for user-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all users - Following DDD: Returns domain entities
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<User>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Getting all users");
            var users = await _userRepository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<User>>.SuccessResponse(users, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, ApiResponse<IEnumerable<User>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Get user by ID - Following DDD: Uses value objects for identity
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    public async Task<ActionResult<ApiResponse<User>>> GetUser(string id)
    {
        try
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);
            var userId = UserId.From(id);
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Create new user - Following DDD: Ensures aggregate consistency
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<User>), 201)]
    [ProducesResponseType(typeof(ApiResponse<User>), 400)]
    public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Email}", request.Email);
            
            // Following DDD: Use value objects for business rules
            var email = Email.From(request.Email);
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<User>.ErrorResponse("User with this email already exists"));
            }

            // Following DDD: Create domain entity with proper validation
            var userRole = UserRole.From(request.Role ?? "Developer");
            var user = new User(
                email,
                request.FirstName,
                request.LastName,
                BCrypt.Net.BCrypt.HashPassword(request.Password),
                userRole
            );

            var createdUser = await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id.Value }, 
                ApiResponse<User>.SuccessResponse(createdUser, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Update user - Following DDD: Ensures aggregate consistency
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<User>), 200)]
    [ProducesResponseType(typeof(ApiResponse<User>), 404)]
    public async Task<ActionResult<ApiResponse<User>>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", id);
            var userId = UserId.From(id);
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found"));
            }

            // Following DDD: Update domain entity while maintaining consistency
            user.UpdateProfile(request.FirstName, request.LastName);
            if (!string.IsNullOrEmpty(request.Role))
            {
                var newRole = UserRole.From(request.Role);
                user.ChangeRole(newRole);
            }

            var updatedUser = await _userRepository.UpdateAsync(user);
            return Ok(ApiResponse<User>.SuccessResponse(updatedUser, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            return StatusCode(500, ApiResponse<User>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Delete user - Following DDD: Handles aggregate deletion
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);
            var userId = UserId.From(id);
            var success = await _userRepository.DeleteAsync(userId);
            
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error"));
        }
    }
}

/// <summary>
/// DTOs for user operations - Following DDD: Keep DTOs simple and focused
/// </summary>
public record CreateUserRequest(string Email, string FirstName, string LastName, string Password, string? Role);
public record UpdateUserRequest(string FirstName, string LastName, string? Role); 