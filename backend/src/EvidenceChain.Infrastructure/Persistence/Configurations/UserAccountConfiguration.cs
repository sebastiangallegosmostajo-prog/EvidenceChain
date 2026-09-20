using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvidenceChain.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration
    : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(
        EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();
    }
}