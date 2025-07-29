using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Project aggregate root
/// Maps value objects to database columns following DDD principles
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Following DDD: Configure aggregate root
        builder.HasKey(p => p.Id);
        
        // Configure value objects as owned entities
        builder.OwnsOne(p => p.Id, projectId =>
        {
            projectId.Property(id => id.Value).HasColumnName("Id");
        });

        builder.OwnsOne(p => p.Name, name =>
        {
            name.Property(n => n.Value).HasColumnName("Name");
        });

        builder.OwnsOne(p => p.Status, status =>
        {
            status.Property(s => s.Value).HasColumnName("Status");
        });

        builder.OwnsOne(p => p.OwnerId, ownerId =>
        {
            ownerId.Property(id => id.Value).HasColumnName("OwnerId");
        });

        // Configure regular properties
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.StartDate).IsRequired(false);
        builder.Property(p => p.EndDate).IsRequired(false);

        // Configure relationships
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.OwnerId.Value)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure indexes
        builder.HasIndex(p => p.Name.Value);
        builder.HasIndex(p => p.Status.Value);
        builder.HasIndex(p => p.OwnerId.Value);

        // Configure table name
        builder.ToTable("Projects");
    }
} 