using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvidenceChain.Infrastructure.Persistence.Configurations;

public sealed class CustodyEventConfiguration
    : IEntityTypeConfiguration<CustodyEvent>
{
    public void Configure(
        EntityTypeBuilder<CustodyEvent> builder)
    {
        builder.ToTable("CustodyEvents");

        builder.HasKey(custodyEvent => custodyEvent.Id);

        builder.Property(custodyEvent => custodyEvent.EventType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(custodyEvent => custodyEvent.Details)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(custodyEvent => custodyEvent.PreviousHash)
            .HasColumnType("char(64)")
            .IsRequired();

        builder.Property(custodyEvent => custodyEvent.Hash)
            .HasColumnType("char(64)")
            .IsRequired();

        builder.Property(custodyEvent => custodyEvent.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(custodyEvent => new
            {
                custodyEvent.EvidenceId,
                custodyEvent.SequenceNumber
            })
            .IsUnique();

        builder.HasIndex(custodyEvent => new
            {
                custodyEvent.EvidenceId,
                custodyEvent.OccurredAtUtc
            });

        builder.HasOne<Evidence>()
            .WithMany()
            .HasForeignKey(custodyEvent => custodyEvent.EvidenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(custodyEvent => custodyEvent.ActorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(custodyEvent =>
                custodyEvent.FromCustodianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(custodyEvent =>
                custodyEvent.ToCustodianId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}