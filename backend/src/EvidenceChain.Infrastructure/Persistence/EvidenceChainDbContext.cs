using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Infrastructure.Persistence;

public sealed class EvidenceChainDbContext
    : DbContext
{
    public EvidenceChainDbContext(
        DbContextOptions<EvidenceChainDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();

    public DbSet<Evidence> Evidences => Set<Evidence>();
    
    public DbSet<CustodyEvent> CustodyEvents => Set<CustodyEvent>();

    private void EnforceAppendOnlyCustodyEvents()
    {
        var hasInvalidChanges = ChangeTracker
            .Entries<CustodyEvent>()
            .Any(entry =>
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted);

        if (hasInvalidChanges)
        {
            throw new InvalidOperationException(
                "Custody events are append-only and cannot be modified or deleted.");
        }
    }
    public override int SaveChanges()
    {
        EnforceAppendOnlyCustodyEvents();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        EnforceAppendOnlyCustodyEvents();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EvidenceChainDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}