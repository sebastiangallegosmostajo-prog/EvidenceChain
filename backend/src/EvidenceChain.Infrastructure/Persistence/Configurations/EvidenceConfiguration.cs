
using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvidenceChain.Infrastructure.Persistence.Configurations;

public sealed class EvidenceConfiguration
    : IEntityTypeConfiguration<Evidence>
{
    public void Configure(
        EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable("Evidence");

        builder.HasKey(evidence => evidence.Id);

        builder.Property(evidence => evidence.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(evidence => evidence.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(evidence => evidence.IntegrityStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(evidence => evidence.CreatedAtUtc)
            .IsRequired();

        builder.Property(evidence => evidence.LastEventAtUtc)
            .IsRequired();

        builder.HasIndex(evidence => evidence.Code)
            .IsUnique();

        builder.HasIndex(evidence => evidence.LastEventAtUtc);

        builder.HasIndex(evidence => evidence.CurrentCustodianId);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(evidence => evidence.CurrentCustodianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}