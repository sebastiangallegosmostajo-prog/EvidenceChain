using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Infrastructure.Persistence.Seeding;

public sealed class EvidenceDataSeeder
{
    private const int EvidenceCount = 1000;
    private const int EventsPerEvidence = 10;

    private static readonly Guid InvestigatorId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    private static readonly Guid CustodianOneId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222222222");

    private static readonly Guid CustodianTwoId =
        Guid.Parse(
            "33333333-3333-3333-3333-333333333333");

    private static readonly Guid TamperedEvidenceId =
        CreateEvidenceId(999);

    private readonly EvidenceChainDbContext _dbContext;

    public EvidenceDataSeeder(
        EvidenceChainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var currentEvidenceCount =
            await _dbContext.Evidences.CountAsync(
                cancellationToken);

        if (currentEvidenceCount == EvidenceCount)
        {
            await EnsureTamperedEventAsync(
                cancellationToken);

            return;
        }

        if (currentEvidenceCount != 0)
        {
            throw new InvalidOperationException(
                $"Se encontraron {currentEvidenceCount} evidencias. " +
                $"El seed esperaba 0 o {EvidenceCount}.");
        }

        var usersExist =
            await _dbContext.Users.CountAsync(
                cancellationToken) >= 4;

        if (!usersExist)
        {
            throw new InvalidOperationException(
                "Los usuarios de demostración deben crearse antes de las evidencias.");
        }

        var seedStart =
            new DateTimeOffset(
                2026,
                1,
                1,
                8,
                0,
                0,
                TimeSpan.Zero);

        for (var index = 1;
             index <= EvidenceCount;
             index++)
        {
            var evidenceId =
                CreateEvidenceId(index);

            var createdAtUtc =
                seedStart.AddMinutes(index * 20L);

            var evidence = new Evidence(
                evidenceId,
                $"EV-{index:0000}",
                $"Evidencia digital forense número {index:0000}.",
                CustodianOneId,
                createdAtUtc);

            _dbContext.Evidences.Add(evidence);

            var sequenceNumber = 0L;

            var previousHash =
                CustodyEvent.GenesisPreviousHash;

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.EvidenceRegistered,
                InvestigatorId,
                null,
                CustodianOneId,
                null,
                createdAtUtc,
                "Evidencia registrada en el sistema.");

            evidence.RegisterEvent(createdAtUtc);

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.IntegrityVerified,
                InvestigatorId,
                CustodianOneId,
                CustodianOneId,
                null,
                createdAtUtc.AddMinutes(1),
                "Verificación inicial de integridad completada.");

            evidence.RegisterEvent(
                createdAtUtc.AddMinutes(1));

            var firstTransfer =
                CreateTransfer(
                    index,
                    1,
                    evidence.Id,
                    CustodianOneId,
                    CustodianTwoId,
                    createdAtUtc.AddMinutes(2),
                    createdAtUtc.AddHours(24));

            _dbContext.CustodyTransfers.Add(
                firstTransfer);

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.TransferRequested,
                InvestigatorId,
                CustodianOneId,
                CustodianTwoId,
                firstTransfer.Id,
                createdAtUtc.AddMinutes(2),
                "Transferencia solicitada al segundo custodio.");

            evidence.RegisterEvent(
                createdAtUtc.AddMinutes(2));

            firstTransfer.Accept(
                CustodianTwoId,
                createdAtUtc.AddMinutes(3));

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.TransferAccepted,
                CustodianTwoId,
                CustodianOneId,
                CustodianTwoId,
                firstTransfer.Id,
                createdAtUtc.AddMinutes(3),
                "Transferencia aceptada por el segundo custodio.");

            evidence.TransferCustody(
                CustodianTwoId,
                createdAtUtc.AddMinutes(3));

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.IntegrityVerified,
                CustodianTwoId,
                CustodianTwoId,
                CustodianTwoId,
                null,
                createdAtUtc.AddMinutes(4),
                "Integridad verificada por el custodio actual.");

            evidence.RegisterEvent(
                createdAtUtc.AddMinutes(4));

            var secondTransfer =
                CreateTransfer(
                    index,
                    2,
                    evidence.Id,
                    CustodianTwoId,
                    CustodianOneId,
                    createdAtUtc.AddMinutes(5),
                    createdAtUtc.AddHours(24));

            _dbContext.CustodyTransfers.Add(
                secondTransfer);

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.TransferRequested,
                InvestigatorId,
                CustodianTwoId,
                CustodianOneId,
                secondTransfer.Id,
                createdAtUtc.AddMinutes(5),
                "Transferencia de retorno solicitada.");

            evidence.RegisterEvent(
                createdAtUtc.AddMinutes(5));

            secondTransfer.Reject(
                CustodianOneId,
                createdAtUtc.AddMinutes(6),
                "El custodio no está disponible.");

            AddEvent(
                index,
                evidence,
                ref sequenceNumber,
                ref previousHash,
                CustodyEventType.TransferRejected,
                CustodianOneId,
                CustodianTwoId,
                CustodianOneId,
                secondTransfer.Id,
                createdAtUtc.AddMinutes(6),
                "Transferencia rechazada por el destinatario.");

            evidence.RegisterEvent(
                createdAtUtc.AddMinutes(6));

            if (index == EvidenceCount)
            {
                AddExpiredPendingTransfer(
                    index,
                    evidence,
                    createdAtUtc,
                    ref sequenceNumber,
                    ref previousHash);
            }
            else
            {
                AddCompletedFinalTransfer(
                    index,
                    evidence,
                    createdAtUtc,
                    ref sequenceNumber,
                    ref previousHash);
            }

            if (sequenceNumber != EventsPerEvidence)
            {
                throw new InvalidOperationException(
                    $"La evidencia {index} generó " +
                    $"{sequenceNumber} eventos en lugar de " +
                    $"{EventsPerEvidence}.");
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await EnsureTamperedEventAsync(
            cancellationToken);
    }

    private void AddCompletedFinalTransfer(
        int evidenceIndex,
        Evidence evidence,
        DateTimeOffset createdAtUtc,
        ref long sequenceNumber,
        ref string previousHash)
    {
        var thirdTransfer =
            CreateTransfer(
                evidenceIndex,
                3,
                evidence.Id,
                CustodianTwoId,
                CustodianOneId,
                createdAtUtc.AddMinutes(7),
                createdAtUtc.AddHours(24));

        _dbContext.CustodyTransfers.Add(
            thirdTransfer);

        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.TransferRequested,
            InvestigatorId,
            CustodianTwoId,
            CustodianOneId,
            thirdTransfer.Id,
            createdAtUtc.AddMinutes(7),
            "Nueva transferencia de retorno solicitada.");

        evidence.RegisterEvent(
            createdAtUtc.AddMinutes(7));

        thirdTransfer.Accept(
            CustodianOneId,
            createdAtUtc.AddMinutes(8));

        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.TransferAccepted,
            CustodianOneId,
            CustodianTwoId,
            CustodianOneId,
            thirdTransfer.Id,
            createdAtUtc.AddMinutes(8),
            "Transferencia de retorno aceptada.");

        evidence.TransferCustody(
            CustodianOneId,
            createdAtUtc.AddMinutes(8));

        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.IntegrityVerified,
            CustodianOneId,
            CustodianOneId,
            CustodianOneId,
            null,
            createdAtUtc.AddMinutes(9),
            "Verificación final de integridad completada.");

        evidence.RegisterEvent(
            createdAtUtc.AddMinutes(9));
    }

    private void AddExpiredPendingTransfer(
        int evidenceIndex,
        Evidence evidence,
        DateTimeOffset createdAtUtc,
        ref long sequenceNumber,
        ref string previousHash)
    {
        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.IntegrityVerified,
            CustodianTwoId,
            CustodianTwoId,
            CustodianTwoId,
            null,
            createdAtUtc.AddMinutes(7),
            "Verificación adicional de integridad.");

        evidence.RegisterEvent(
            createdAtUtc.AddMinutes(7));

        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.IntegrityVerified,
            CustodianTwoId,
            CustodianTwoId,
            CustodianTwoId,
            null,
            createdAtUtc.AddMinutes(8),
            "Segunda verificación adicional de integridad.");

        evidence.RegisterEvent(
            createdAtUtc.AddMinutes(8));

        var expiredTransfer =
            CreateTransfer(
                evidenceIndex,
                3,
                evidence.Id,
                CustodianTwoId,
                CustodianOneId,
                createdAtUtc.AddMinutes(9),
                createdAtUtc.AddMinutes(10));

        _dbContext.CustodyTransfers.Add(
            expiredTransfer);

        AddEvent(
            evidenceIndex,
            evidence,
            ref sequenceNumber,
            ref previousHash,
            CustodyEventType.TransferRequested,
            InvestigatorId,
            CustodianTwoId,
            CustodianOneId,
            expiredTransfer.Id,
            createdAtUtc.AddMinutes(9),
            "Transferencia pendiente fuera del plazo configurado.");

        evidence.RegisterEvent(
            createdAtUtc.AddMinutes(9));
    }

    private void AddEvent(
        int evidenceIndex,
        Evidence evidence,
        ref long sequenceNumber,
        ref string previousHash,
        CustodyEventType eventType,
        Guid actorId,
        Guid? fromCustodianId,
        Guid? toCustodianId,
        Guid? transferId,
        DateTimeOffset occurredAtUtc,
        string details)
    {
        sequenceNumber++;

        var eventId =
            CreateEventId(
                evidenceIndex,
                (int)sequenceNumber);

        var custodyEvent = CustodyEvent.Create(
            eventId,
            evidence.Id,
            sequenceNumber,
            eventType,
            actorId,
            fromCustodianId,
            toCustodianId,
            transferId,
            occurredAtUtc,
            details,
            previousHash);

        _dbContext.CustodyEvents.Add(
            custodyEvent);

        previousHash =
            custodyEvent.Hash;
    }

    private static CustodyTransfer CreateTransfer(
        int evidenceIndex,
        int transferNumber,
        Guid evidenceId,
        Guid fromCustodianId,
        Guid toCustodianId,
        DateTimeOffset requestedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        var transfer = CustodyTransfer.Request(
            evidenceId,
            fromCustodianId,
            toCustodianId,
            InvestigatorId,
            requestedAtUtc,
            expiresAtUtc);

        SetDeterministicTransferId(
            transfer,
            CreateTransferId(
                evidenceIndex,
                transferNumber));

        return transfer;
    }

    private static void SetDeterministicTransferId(
        CustodyTransfer transfer,
        Guid deterministicId)
    {
        var idProperty =
            typeof(CustodyTransfer)
                .GetProperty(nameof(CustodyTransfer.Id));

        idProperty?.SetValue(
            transfer,
            deterministicId);
    }

    private async Task EnsureTamperedEventAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.Database
            .ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE [CustodyEvents]
                SET [Details] = N'Contenido alterado deliberadamente para probar la cadena.'
                WHERE [EvidenceId] = {TamperedEvidenceId}
                  AND [SequenceNumber] = 5
                """,
                cancellationToken);
    }

    private static Guid CreateEvidenceId(
        int index)
    {
        return Guid.Parse(
            $"aaaaaaaa-aaaa-aaaa-aaaa-{index:000000000000}");
    }

    private static Guid CreateEventId(
        int evidenceIndex,
        int sequenceNumber)
    {
        var value =
            ((evidenceIndex - 1) * EventsPerEvidence) +
            sequenceNumber;

        return Guid.Parse(
            $"bbbbbbbb-bbbb-bbbb-bbbb-{value:000000000000}");
    }

    private static Guid CreateTransferId(
        int evidenceIndex,
        int transferNumber)
    {
        var value =
            ((evidenceIndex - 1) * 3) +
            transferNumber;

        return Guid.Parse(
            $"cccccccc-cccc-cccc-cccc-{value:000000000000}");
    }
}