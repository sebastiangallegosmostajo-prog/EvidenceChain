namespace EvidenceChain.Application.CustodyTransfers.Request;

public sealed record RequestCustodyTransferResult(
    Guid TransferId,
    Guid EvidenceId,
    Guid FromCustodianId,
    Guid ToCustodianId,
    string Status,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string RowVersion);