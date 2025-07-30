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
        
        // Configure value objects with conversions
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.From(value))
            .HasColumnName("Email")
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion(
                role => role.Value,
                value => UserRole.From(value))
            .HasColumnName("Role")
            .IsRequired();

        builder.Property(u => u.Status)
            .HasConversion(
                status => status.Value,
                value => UserStatus.From(value))
            .HasColumnName("Status")
            .IsRequired();

        // Configure regular properties
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
        builder.Property(u => u.LastLoginAt).IsRequired(false);

        // Configure indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.Status);

        // Configure table name
        builder.ToTable("Users");
    }
} 