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

        // Configure value objects with conversions
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TaskId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasConversion(
                title => title.Value,
                value => TaskTitle.From(value))
            .HasColumnName("Title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion(
                status => status.Value,
                value => TaskState.From(value))
            .HasColumnName("Status")
            .IsRequired();

        builder.Property(t => t.ProjectId)
            .HasConversion(
                projectId => projectId.Value,
                value => ProjectId.From(value))
            .HasColumnName("ProjectId")
            .IsRequired();

        // Configure regular properties
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.AssigneeId).HasMaxLength(36).IsRequired(false);
        builder.Property(t => t.CreatedBy).HasMaxLength(36).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        // Configure relationships
        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CreatedBy);
        builder.HasIndex(t => t.CreatedAt);

        // Configure table name
        builder.ToTable("Tasks");
    }
} 