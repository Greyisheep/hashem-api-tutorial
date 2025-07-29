using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Task aggregate root
/// Maps value objects to database columns following DDD principles
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<TaskFlow.Domain.Entities.Task>
{
    public void Configure(EntityTypeBuilder<TaskFlow.Domain.Entities.Task> builder)
    {
        // Following DDD: Configure aggregate root
        builder.HasKey(t => t.Id);

        // Configure value objects as owned entities
        builder.OwnsOne(t => t.Id, taskId =>
        {
            taskId.Property(id => id.Value).HasColumnName("Id");
        });

        builder.OwnsOne(t => t.Title, title =>
        {
            title.Property(t => t.Value).HasColumnName("Title").HasMaxLength(200);
        });

        builder.OwnsOne(t => t.Status, status =>
        {
            status.Property(s => s.Value).HasColumnName("Status");
        });

        builder.OwnsOne(t => t.ProjectId, projectId =>
        {
            projectId.Property(id => id.Value).HasColumnName("ProjectId");
        });

        // Configure regular properties
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.AssigneeId).HasMaxLength(36).IsRequired(false);
        builder.Property(t => t.CreatedBy).HasMaxLength(36).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // Configure relationships
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(t => t.ProjectId.Value)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(t => t.ProjectId.Value);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.Status.Value);
        builder.HasIndex(t => t.CreatedBy);
        builder.HasIndex(t => t.CreatedAt);

        // Configure table name
        builder.ToTable("Tasks");
    }
} 