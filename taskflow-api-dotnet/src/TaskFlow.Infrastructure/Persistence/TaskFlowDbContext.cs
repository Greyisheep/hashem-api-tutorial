using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence;

public class TaskFlowDbContext : DbContext, IUnitOfWork
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
    {
    }

    // Domain Entities - Following DDD principles with proper aggregate roots
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskFlow.Domain.Entities.Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
        
        // Seed initial data following DDD principles
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Users (Aggregate Root) - Using anonymous objects for EF Core seeding
        var adminUserId = Guid.NewGuid().ToString();
        var projectManagerId = Guid.NewGuid().ToString();
        var developerId = Guid.NewGuid().ToString();

        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = adminUserId,
                Email = "admin@taskflow.com",
                FirstName = "Admin",
                LastName = "User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = projectManagerId,
                Email = "pm@taskflow.com",
                FirstName = "Project",
                LastName = "Manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pm123"),
                Role = "ProjectManager",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = developerId,
                Email = "dev@taskflow.com",
                FirstName = "John",
                LastName = "Developer",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("dev123"),
                Role = "Developer",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Projects (Aggregate Root) - Using anonymous objects for EF Core seeding
        var mainProjectId = Guid.NewGuid().ToString();
        var demoProjectId = Guid.NewGuid().ToString();

        modelBuilder.Entity<Project>().HasData(
            new
            {
                Id = mainProjectId,
                Name = "TaskFlow API Development",
                Description = "Main project for developing the TaskFlow API with .NET and DDD principles",
                Status = "Planning",
                OwnerId = adminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = demoProjectId,
                Name = "Demo Project",
                Description = "A demo project to showcase TaskFlow capabilities",
                Status = "Planning",
                OwnerId = projectManagerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Tasks (Aggregate Root) - Using anonymous objects for EF Core seeding
        modelBuilder.Entity<TaskFlow.Domain.Entities.Task>().HasData(
            new
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Implement Authentication",
                Description = "Implement JWT-based authentication with proper security measures",
                Status = "pending",
                ProjectId = mainProjectId,
                CreatedBy = developerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Design Database Schema",
                Description = "Create comprehensive database schema following DDD principles",
                Status = "completed",
                ProjectId = mainProjectId,
                CreatedBy = projectManagerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Setup CI/CD Pipeline",
                Description = "Configure automated testing and deployment pipeline",
                Status = "pending",
                ProjectId = mainProjectId,
                CreatedBy = adminUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit trail logic here if needed
        return await base.SaveChangesAsync(cancellationToken);
    }
} 