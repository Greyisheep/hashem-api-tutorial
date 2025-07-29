using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for User aggregate root
/// Maps value objects to database columns following DDD principles
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Following DDD: Configure aggregate root
        builder.HasKey(u => u.Id);
        
        // Configure value objects as owned entities
        builder.OwnsOne(u => u.Id, userId =>
        {
            userId.Property(id => id.Value).HasColumnName("Id");
        });

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email");
        });

        builder.OwnsOne(u => u.Role, role =>
        {
            role.Property(r => r.Value).HasColumnName("Role");
        });

        builder.OwnsOne(u => u.Status, status =>
        {
            status.Property(s => s.Value).HasColumnName("Status");
        });

        // Configure regular properties
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
        builder.Property(u => u.LastLoginAt).IsRequired(false);

        // Configure indexes
        builder.HasIndex(u => u.Email.Value).IsUnique();
        builder.HasIndex(u => u.Role.Value);
        builder.HasIndex(u => u.Status.Value);

        // Configure table name
        builder.ToTable("Users");
    }
} 