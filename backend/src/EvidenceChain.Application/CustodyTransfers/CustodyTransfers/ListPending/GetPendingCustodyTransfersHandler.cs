using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application
    .CustodyTransfers.ListPending;

public sealed class
    GetPendingCustodyTransfersHandler
{
    private readonly IEvidenceChainDbContext
        _dbContext;

    private readonly TimeProvider
        _timeProvider;

    public GetPendingCustodyTransfersHandler(
        IEvidenceChainDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<
        PendingCustodyTransfersResult>
        HandleAsync(
            Guid custodianId,
            CancellationToken cancellationToken =
                default)
    {
        if (custodianId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del custodio es obligatorio.",
                nameof(custodianId));
        }

        var rows =
            await _dbContext.CustodyTransfers
                .AsNoTracking()
                .Where(transfer =>
                    transfer.ToCustodianId ==
                        custodianId &&
                    transfer.Status ==
                        TransferStatus.Pending)
                .OrderBy(transfer =>
                    transfer.ExpiresAtUtc)
                .Select(transfer =>
                    new
                    {
                        TransferId =
                            transfer.Id,

                        transfer.EvidenceId,

                        EvidenceCode =
                            _dbContext.Evidences
                                .Where(evidence =>
                                    evidence.Id ==
                                    transfer.EvidenceId)
                                .Select(evidence =>
                                    evidence.Code)
                                .FirstOrDefault()
                            ?? "Evidencia desconocida",

                        transfer.FromCustodianId,

                        FromCustodianName =
                            _dbContext.Users
                                .Where(user =>
                                    user.Id ==
                                    transfer.FromCustodianId)
                                .Select(user =>
                                    user.Name)
                                .FirstOrDefault()
                            ?? "Custodio desconocido",

                        transfer.ToCustodianId,
                        transfer.RequestedById,

                        RequestedByName =
                            _dbContext.Users
                                .Where(user =>
                                    user.Id ==
                                    transfer.RequestedById)
                                .Select(user =>
                                    user.Name)
                                .FirstOrDefault()
                            ?? "Usuario desconocido",

                        transfer.RequestedAtUtc,
                        transfer.ExpiresAtUtc,
                        transfer.Status,
                        transfer.RowVersion
                    })
                .ToListAsync(
                    cancellationToken);

        var now =
            _timeProvider.GetUtcNow();

        var items =
            rows
                .Select(row =>
                    new PendingCustodyTransferItem(
                        row.TransferId,
                        row.EvidenceId,
                        row.EvidenceCode,
                        row.FromCustodianId,
                        row.FromCustodianName,
                        row.ToCustodianId,
                        row.RequestedById,
                        row.RequestedByName,
                        row.RequestedAtUtc,
                        row.ExpiresAtUtc,
                        row.ExpiresAtUtc < now,
                        row.Status.ToString(),
                        Convert.ToBase64String(
                            row.RowVersion)))
                .ToList();

        return new PendingCustodyTransfersResult(
            items,
            items.Count);
    }
}