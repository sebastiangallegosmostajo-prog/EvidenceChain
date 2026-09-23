using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Evidences.Chain;

public sealed class GetEvidenceChainHandler
{
    private readonly IEvidenceChainDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public GetEvidenceChainHandler(
        IEvidenceChainDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<EvidenceChainResult> HandleAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        if (evidenceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de evidencia es obligatorio.",
                nameof(evidenceId));
        }

        var evidence =
            await _dbContext.Evidences
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Id == evidenceId,
                    cancellationToken);

        if (evidence is null)
        {
            throw new NotFoundException(
                $"No se encontró la evidencia {evidenceId}.");
        }

        var events =
            await _dbContext.CustodyEvents
                .AsNoTracking()
                .Where(item =>
                    item.EvidenceId == evidenceId)
                .OrderBy(item =>
                    item.SequenceNumber)
                .ToListAsync(
                    cancellationToken);

        var now =
            _timeProvider.GetUtcNow();

        var expiredTransfers =
            await _dbContext.CustodyTransfers
                .AsNoTracking()
                .Where(transfer =>
                    transfer.EvidenceId == evidenceId &&
                    transfer.Status == TransferStatus.Pending &&
                    transfer.ExpiresAtUtc < now)
                .OrderBy(transfer =>
                    transfer.ExpiresAtUtc)
                .ToListAsync(
                    cancellationToken);

        var userIds =
            new HashSet<Guid>();

        foreach (var custodyEvent in events)
        {
            userIds.Add(
                custodyEvent.ActorId);

            if (custodyEvent.FromCustodianId.HasValue)
            {
                userIds.Add(
                    custodyEvent.FromCustodianId.Value);
            }

            if (custodyEvent.ToCustodianId.HasValue)
            {
                userIds.Add(
                    custodyEvent.ToCustodianId.Value);
            }
        }

        foreach (var transfer in expiredTransfers)
        {
            userIds.Add(
                transfer.FromCustodianId);

            userIds.Add(
                transfer.ToCustodianId);

            userIds.Add(
                transfer.RequestedById);
        }

        var users =
            await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                    userIds.Contains(user.Id))
                .Select(user =>
                    new
                    {
                        user.Id,
                        user.Name
                    })
                .ToDictionaryAsync(
                    user => user.Id,
                    user => user.Name,
                    cancellationToken);

        var anomalies =
            expiredTransfers
                .Select(transfer =>
                    new EvidenceAnomalyItem(
                        "ExpiredPendingTransfer",
                        "High",

                        $"La transferencia al custodio " +
                        $"{GetUserName(users, transfer.ToCustodianId)} " +
                        $"venció el " +
                        $"{transfer.ExpiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC " +
                        $"y continúa pendiente.",

                        transfer.Id,
                        transfer.RequestedAtUtc,
                        transfer.ExpiresAtUtc))
                .ToList();

        var anomaliesByTransferId =
            anomalies.ToDictionary(
                anomaly => anomaly.TransferId);

        var eventItems =
            events
                .Select(custodyEvent =>
                {
                    EvidenceAnomalyItem? anomaly =
                        null;

                    if (custodyEvent.TransferId.HasValue)
                    {
                        anomaliesByTransferId.TryGetValue(
                            custodyEvent.TransferId.Value,
                            out anomaly);
                    }

                    return new EvidenceChainEventItem(
                        custodyEvent.Id,
                        custodyEvent.SequenceNumber,
                        custodyEvent.EventType.ToString(),
                        custodyEvent.ActorId,

                        GetUserName(
                            users,
                            custodyEvent.ActorId),

                        custodyEvent.FromCustodianId,

                        GetOptionalUserName(
                            users,
                            custodyEvent.FromCustodianId),

                        custodyEvent.ToCustodianId,

                        GetOptionalUserName(
                            users,
                            custodyEvent.ToCustodianId),

                        custodyEvent.TransferId,
                        custodyEvent.OccurredAtUtc,
                        custodyEvent.Details,
                        custodyEvent.PreviousHash,
                        custodyEvent.Hash,
                        anomaly is not null,
                        anomaly?.Severity,
                        anomaly?.Explanation);
                })
                .ToList();

        return new EvidenceChainResult(
            evidence.Id,
            evidence.Code,
            evidence.IntegrityStatus.ToString(),
            eventItems,
            anomalies);
    }

    private static string GetUserName(
        IReadOnlyDictionary<Guid, string> users,
        Guid userId)
    {
        return users.TryGetValue(
            userId,
            out var name)
                ? name
                : "Usuario desconocido";
    }

    private static string? GetOptionalUserName(
        IReadOnlyDictionary<Guid, string> users,
        Guid? userId)
    {
        if (!userId.HasValue)
        {
            return null;
        }

        return GetUserName(
            users,
            userId.Value);
    }
}