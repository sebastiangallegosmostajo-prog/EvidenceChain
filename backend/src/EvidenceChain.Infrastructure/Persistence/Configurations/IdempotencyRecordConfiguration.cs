using EvidenceChain.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvidenceChain.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(
        EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(record => record.Operation)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(record => record.RequestHash)
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(record => record.ResourceId)
            .IsRequired();

        builder.Property(record => record.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(record => new
            {
                record.Operation,
                record.Key
            })
            .IsUnique()
            .HasDatabaseName(
                "UX_IdempotencyRecords_Operation_Key");

        builder.HasIndex(record => record.CreatedAtUtc);
    }
}