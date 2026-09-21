using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvidenceChain.Infrastructure.Persistence.Configurations;

public sealed class CustodyTransferConfiguration
    : IEntityTypeConfiguration<CustodyTransfer>
{
    public void Configure(
        EntityTypeBuilder<CustodyTransfer> builder)
    {
        builder.ToTable(
            "CustodyTransfers",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_CustodyTransfers_DifferentCustodians",
                    "[FromCustodianId] <> [ToCustodianId]");

                table.HasCheckConstraint(
                    "CK_CustodyTransfers_ValidExpiration",
                    "[ExpiresAtUtc] > [RequestedAtUtc]");
            });

        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transfer => transfer.RequestedAtUtc)
            .IsRequired();

        builder.Property(transfer => transfer.ExpiresAtUtc)
            .IsRequired();

        builder.Property(transfer => transfer.RejectionReason)
            .HasMaxLength(500);

        builder.Property(transfer => transfer.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne<Evidence>()
            .WithMany()
            .HasForeignKey(transfer => transfer.EvidenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(transfer => transfer.FromCustodianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(transfer => transfer.ToCustodianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(transfer => transfer.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => new
        {
            transfer.EvidenceId,
            transfer.Status
        });

        builder.HasIndex(transfer => new
        {
            transfer.ToCustodianId,
            transfer.Status
        });
    }
}