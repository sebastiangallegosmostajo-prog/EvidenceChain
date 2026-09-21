using System.Security.Cryptography;
using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Application.CustodyTransfers.Common;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.CustodyTransfers.Accept;

public sealed class AcceptCustodyTransferHandler
{
    private readonly IEvidenceChainDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public AcceptCustodyTransferHandler(
        IEvidenceChainDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<CustodyTransferActionResult> HandleAsync(
        AcceptCustodyTransferCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.TransferId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de transferencia es obligatorio.");
        }

        if (command.ActorId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del usuario es obligatorio.");
        }

        if (command.ExpectedRowVersion.Length == 0)
        {
            throw new ArgumentException(
                "If-Match debe contener una versión válida.");
        }

        var transfer = await _dbContext.CustodyTransfers
            .SingleOrDefaultAsync(
                item => item.Id == command.TransferId,
                cancellationToken);

        if (transfer is null)
        {
            throw new NotFoundException(
                $"No se encontró la transferencia {command.TransferId}.");
        }

        if (!CryptographicOperations.FixedTimeEquals(
                transfer.RowVersion,
                command.ExpectedRowVersion))
        {
            throw new ConcurrencyConflictException(
                "La transferencia fue modificada por otro usuario.",
                ToResult(transfer));
        }

        var evidence = await _dbContext.Evidences
            .SingleOrDefaultAsync(
                item => item.Id == transfer.EvidenceId,
                cancellationToken);

        if (evidence is null)
        {
            throw new NotFoundException(
                $"No se encontró la evidencia {transfer.EvidenceId}.");
        }

        var lastEvent = await _dbContext.CustodyEvents
            .AsNoTracking()
            .Where(item =>
                item.EvidenceId == transfer.EvidenceId)
            .OrderByDescending(item =>
                item.SequenceNumber)
            .Select(item => new
            {
                item.SequenceNumber,
                item.Hash
            })
            .FirstOrDefaultAsync(cancellationToken);

        var sequenceNumber =
            lastEvent is null
                ? 1
                : lastEvent.SequenceNumber + 1;

        var previousHash =
            lastEvent?.Hash ??
            CustodyEvent.GenesisPreviousHash;

        var respondedAtUtc =
            _timeProvider.GetUtcNow();

        transfer.Accept(
            command.ActorId,
            respondedAtUtc);

        evidence.TransferCustody(
            transfer.ToCustodianId,
            respondedAtUtc);

        var custodyEvent = CustodyEvent.Create(
            Guid.NewGuid(),
            transfer.EvidenceId,
            sequenceNumber,
            CustodyEventType.TransferAccepted,
            command.ActorId,
            transfer.FromCustodianId,
            transfer.ToCustodianId,
            transfer.Id,
            respondedAtUtc,
            "Transferencia de custodia aceptada.",
            previousHash);

        _dbContext.CustodyEvents.Add(
            custodyEvent);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            var currentTransfer =
                await _dbContext.CustodyTransfers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item =>
                            item.Id == command.TransferId,
                        cancellationToken);

            if (currentTransfer is null)
            {
                throw new NotFoundException(
                    $"La transferencia {command.TransferId} ya no existe.");
            }

            throw new ConcurrencyConflictException(
                "La transferencia fue modificada mientras se procesaba la aceptación.",
                ToResult(currentTransfer));
        }

        return ToResult(transfer);
    }

    private static CustodyTransferActionResult ToResult(
        CustodyTransfer transfer)
    {
        return new CustodyTransferActionResult(
            transfer.Id,
            transfer.EvidenceId,
            transfer.FromCustodianId,
            transfer.ToCustodianId,
            transfer.Status.ToString(),
            transfer.RespondedAtUtc,
            transfer.RejectionReason,
            Convert.ToBase64String(
                transfer.RowVersion));
    }
}