namespace EvidenceChain.Application
    .CustodyTransfers.ListPending;

public sealed record PendingCustodyTransferItem(
    Guid TransferId,
    Guid EvidenceId,
    string EvidenceCode,
    Guid FromCustodianId,
    string FromCustodianName,
    Guid ToCustodianId,
    Guid RequestedById,
    string RequestedByName,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    bool IsExpired,
    string Status,
    string RowVersion);

public sealed record PendingCustodyTransfersResult(
    IReadOnlyList<
        PendingCustodyTransferItem> Items,
    int TotalCount);