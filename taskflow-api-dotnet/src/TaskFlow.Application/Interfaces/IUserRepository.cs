using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Repository interface for User aggregate root following DDD principles
/// Users are aggregate roots that encapsulate user identity and authentication
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get all users (for demo purposes - in production, consider pagination)
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email (for authentication)
    /// </summary>
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new user
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing user
    /// </summary>
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user by ID
    /// </summary>
    Task<bool> DeleteAsync(UserId id, CancellationToken cancellationToken = default);
} 