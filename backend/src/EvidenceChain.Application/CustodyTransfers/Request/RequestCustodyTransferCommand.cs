namespace EvidenceChain.Application.CustodyTransfers.Request;

public sealed record RequestCustodyTransferCommand(
    Guid EvidenceId,
    Guid ToCustodianId,
    Guid RequestedById,
    string IdempotencyKey);