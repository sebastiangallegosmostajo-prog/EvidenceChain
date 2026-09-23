namespace EvidenceChain.Application.CustodyTransfers.Reject;

public sealed record RejectCustodyTransferCommand(
    Guid TransferId,
    Guid ActorId,
    string Reason,
    byte[] ExpectedRowVersion);