namespace EvidenceChain.Application.CustodyTransfers.Common;

public sealed record CustodyTransferActionResult(
    Guid TransferId,
    Guid EvidenceId,
    Guid FromCustodianId,
    Guid ToCustodianId,
    string Status,
    DateTimeOffset? RespondedAtUtc,
    string? RejectionReason,
    string RowVersion);