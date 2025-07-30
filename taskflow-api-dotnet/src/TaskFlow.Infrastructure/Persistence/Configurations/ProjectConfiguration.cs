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
        
        // Configure value objects with conversions
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ProjectId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasConversion(
                name => name.Value,
                value => ProjectName.From(value))
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion(
                status => status.Value,
                value => ProjectStatus.From(value))
            .HasColumnName("Status")
            .IsRequired();

        builder.Property(p => p.OwnerId)
            .HasConversion(
                ownerId => ownerId.Value,
                value => UserId.From(value))
            .HasColumnName("OwnerId")
            .IsRequired();

        // Configure regular properties
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
        builder.Property(p => p.StartDate).IsRequired(false);
        builder.Property(p => p.EndDate).IsRequired(false);

        // Configure relationships
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Members collection as a separate table
        builder.OwnsMany(p => p.Members, members =>
        {
            members.ToTable("ProjectMembers");
            members.Property(m => m.Value).HasColumnName("UserId");
        });

        // Configure indexes
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OwnerId);

        // Configure table name
        builder.ToTable("Projects");
    }
} 