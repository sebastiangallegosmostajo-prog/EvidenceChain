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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(EvidenceChainDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}