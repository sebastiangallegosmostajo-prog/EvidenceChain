using System.Security.Cryptography;
using System.Text;
using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Application.Common.Models;
using EvidenceChain.Application.Common.Options;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EvidenceChain.Application.CustodyTransfers.Request;

public sealed class RequestCustodyTransferHandler
{
    private const string OperationName =
        "RequestCustodyTransfer";

    private readonly IEvidenceChainDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly CustodyTransferOptions _options;

    public RequestCustodyTransferHandler(
        IEvidenceChainDbContext dbContext,
        TimeProvider timeProvider,
        IOptions<CustodyTransferOptions> options)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _options = options.Value;
    }

    public async Task<RequestCustodyTransferResult> HandleAsync(
        RequestCustodyTransferCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateOptions();
        ValidateCommand(command);

        var idempotencyKey =
            command.IdempotencyKey.Trim();

        var requestHash =
            CalculateRequestHash(command);

        var existingIdempotencyRecord =
            await _dbContext.IdempotencyRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    record =>
                        record.Operation == OperationName &&
                        record.Key == idempotencyKey,
                    cancellationToken);

        if (existingIdempotencyRecord is not null)
        {
            return await HandleRepeatedRequestAsync(
                existingIdempotencyRecord,
                requestHash,
                cancellationToken);
        }

        var evidence = await _dbContext.Evidences
            .SingleOrDefaultAsync(
                item => item.Id == command.EvidenceId,
                cancellationToken);

        if (evidence is null)
        {
            throw new NotFoundException(
                $"No se encontró la evidencia {command.EvidenceId}.");
        }

        var destinationCustodianExists =
            await _dbContext.Users.AnyAsync(
                user => user.Id == command.ToCustodianId,
                cancellationToken);

        if (!destinationCustodianExists)
        {
            throw new NotFoundException(
                $"No se encontró el custodio {command.ToCustodianId}.");
        }

        var requestingUserExists =
            await _dbContext.Users.AnyAsync(
                user => user.Id == command.RequestedById,
                cancellationToken);

        if (!requestingUserExists)
        {
            throw new NotFoundException(
                $"No se encontró el usuario solicitante {command.RequestedById}.");
        }

        var hasPendingTransfer =
            await _dbContext.CustodyTransfers.AnyAsync(
                transfer =>
                    transfer.EvidenceId == command.EvidenceId &&
                    transfer.Status == TransferStatus.Pending,
                cancellationToken);

        if (hasPendingTransfer)
        {
            throw new ConflictException(
                "La evidencia ya tiene una transferencia pendiente.");
        }

        var lastEvent = await _dbContext.CustodyEvents
            .AsNoTracking()
            .Where(item =>
                item.EvidenceId == command.EvidenceId)
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

        var requestedAtUtc =
            _timeProvider.GetUtcNow();

        var expiresAtUtc =
            requestedAtUtc.AddHours(
                _options.PendingExpirationHours);

        var transfer = CustodyTransfer.Request(
            evidence.Id,
            evidence.CurrentCustodianId,
            command.ToCustodianId,
            command.RequestedById,
            requestedAtUtc,
            expiresAtUtc);

        var custodyEvent = CustodyEvent.Create(
            Guid.NewGuid(),
            evidence.Id,
            sequenceNumber,
            CustodyEventType.TransferRequested,
            command.RequestedById,
            evidence.CurrentCustodianId,
            command.ToCustodianId,
            transfer.Id,
            requestedAtUtc,
            "Transferencia de custodia solicitada.",
            previousHash);

        var idempotencyRecord =
            IdempotencyRecord.Create(
                idempotencyKey,
                OperationName,
                requestHash,
                transfer.Id,
                requestedAtUtc);

        evidence.RegisterEvent(requestedAtUtc);

        _dbContext.CustodyTransfers.Add(transfer);
        _dbContext.CustodyEvents.Add(custodyEvent);
        _dbContext.IdempotencyRecords.Add(
            idempotencyRecord);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ToResult(transfer);
    }

    private async Task<RequestCustodyTransferResult>
        HandleRepeatedRequestAsync(
            IdempotencyRecord record,
            string currentRequestHash,
            CancellationToken cancellationToken)
    {
        if (!string.Equals(
                record.RequestHash,
                currentRequestHash,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "La Idempotency-Key ya fue utilizada con datos diferentes.");
        }

        var existingTransfer =
            await _dbContext.CustodyTransfers
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    transfer =>
                        transfer.Id == record.ResourceId,
                    cancellationToken);

        if (existingTransfer is null)
        {
            throw new InvalidOperationException(
                "El registro de idempotencia referencia una transferencia inexistente.");
        }

        return ToResult(existingTransfer);
    }

    private static string CalculateRequestHash(
        RequestCustodyTransferCommand command)
    {
        var canonicalRequest = string.Join(
            "|",
            command.EvidenceId.ToString("N"),
            command.ToCustodianId.ToString("N"),
            command.RequestedById.ToString("N"));

        var requestBytes =
            Encoding.UTF8.GetBytes(canonicalRequest);

        var hashBytes =
            SHA256.HashData(requestBytes);

        return Convert
            .ToHexString(hashBytes)
            .ToLowerInvariant();
    }

    private static RequestCustodyTransferResult ToResult(
        CustodyTransfer transfer)
    {
        return new RequestCustodyTransferResult(
            transfer.Id,
            transfer.EvidenceId,
            transfer.FromCustodianId,
            transfer.ToCustodianId,
            transfer.Status.ToString(),
            transfer.RequestedAtUtc,
            transfer.ExpiresAtUtc,
            Convert.ToBase64String(
                transfer.RowVersion));
    }

    private static void ValidateCommand(
        RequestCustodyTransferCommand command)
    {
        if (command.EvidenceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de evidencia es obligatorio.");
        }

        if (command.ToCustodianId == Guid.Empty)
        {
            throw new ArgumentException(
                "El custodio destinatario es obligatorio.");
        }

        if (command.RequestedById == Guid.Empty)
        {
            throw new ArgumentException(
                "El usuario solicitante es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(
                command.IdempotencyKey))
        {
            throw new ArgumentException(
                "La cabecera Idempotency-Key es obligatoria.");
        }

        if (command.IdempotencyKey.Trim().Length > 100)
        {
            throw new ArgumentException(
                "La cabecera Idempotency-Key no puede superar 100 caracteres.");
        }
    }

    private void ValidateOptions()
    {
        if (_options.PendingExpirationHours is < 1 or > 168)
        {
            throw new InvalidOperationException(
                "CustodyTransfers:PendingExpirationHours debe estar entre 1 y 168.");
        }
    }
}