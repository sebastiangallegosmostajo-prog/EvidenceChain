using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Evidences.VerifyChain;

public sealed class VerifyEvidenceChainHandler
{
    private readonly IEvidenceChainDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public VerifyEvidenceChainHandler(
        IEvidenceChainDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<ChainVerificationResult> HandleAsync(
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

        Guid? firstInvalidEventId = null;
        long? firstInvalidSequenceNumber = null;
        string? failureReason = null;
        string? expectedPreviousHash = null;
        string? actualPreviousHash = null;
        string? storedHash = null;
        string? calculatedHash = null;

        if (events.Count == 0)
        {
            failureReason =
                "La evidencia no contiene eventos de custodia.";
        }
        else
        {
            var expectedSequenceNumber =
                1L;

            var previousHash =
                CustodyEvent.GenesisPreviousHash;

            foreach (var custodyEvent in events)
            {
                if (custodyEvent.SequenceNumber !=
                    expectedSequenceNumber)
                {
                    firstInvalidEventId =
                        custodyEvent.Id;

                    firstInvalidSequenceNumber =
                        custodyEvent.SequenceNumber;

                    failureReason =
                        $"Se esperaba la secuencia " +
                        $"{expectedSequenceNumber}, pero se encontró " +
                        $"{custodyEvent.SequenceNumber}.";

                    break;
                }

                if (!string.Equals(
                        custodyEvent.PreviousHash,
                        previousHash,
                        StringComparison.OrdinalIgnoreCase))
                {
                    firstInvalidEventId =
                        custodyEvent.Id;

                    firstInvalidSequenceNumber =
                        custodyEvent.SequenceNumber;

                    failureReason =
                        "El PreviousHash no coincide con el hash del evento anterior.";

                    expectedPreviousHash =
                        previousHash;

                    actualPreviousHash =
                        custodyEvent.PreviousHash;

                    break;
                }

                var recalculatedHash =
                    custodyEvent.RecalculateHash();

                if (!string.Equals(
                        custodyEvent.Hash,
                        recalculatedHash,
                        StringComparison.OrdinalIgnoreCase))
                {
                    firstInvalidEventId =
                        custodyEvent.Id;

                    firstInvalidSequenceNumber =
                        custodyEvent.SequenceNumber;

                    failureReason =
                        "El contenido del evento no coincide con su hash almacenado.";

                    storedHash =
                        custodyEvent.Hash;

                    calculatedHash =
                        recalculatedHash;

                    break;
                }

                previousHash =
                    custodyEvent.Hash;

                expectedSequenceNumber++;
            }
        }

        var isIntact =
            failureReason is null;

        var integrityStatus =
            isIntact
                ? IntegrityStatus.Intact
                : IntegrityStatus.Compromised;

        evidence.UpdateIntegrityStatus(
            integrityStatus);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new ChainVerificationResult(
            evidence.Id,
            isIntact,
            integrityStatus.ToString(),
            events.Count,
            firstInvalidEventId,
            firstInvalidSequenceNumber,
            failureReason,
            expectedPreviousHash,
            actualPreviousHash,
            storedHash,
            calculatedHash,
            _timeProvider.GetUtcNow());
    }
}