using EvidenceChain.Application.Common.Models;
using EvidenceChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Common.Interfaces;

public interface IEvidenceChainDbContext
{
    DbSet<UserAccount> Users { get; }

    DbSet<Evidence> Evidences { get; }

    DbSet<CustodyEvent> CustodyEvents { get; }

    DbSet<CustodyTransfer> CustodyTransfers { get; }

    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}