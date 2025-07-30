using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence;

public class TaskFlowDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
    {
    }

    // Domain Entities - Following DDD principles with proper aggregate roots
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskFlow.Domain.Entities.Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base method to configure Identity tables
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
        
        // Seed initial data following DDD principles
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Note: Seed data is handled in Program.cs after database creation
        // This approach avoids EF Core value object conversion issues during model building
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit trail logic here if needed
        return await base.SaveChangesAsync(cancellationToken);
    }
} 