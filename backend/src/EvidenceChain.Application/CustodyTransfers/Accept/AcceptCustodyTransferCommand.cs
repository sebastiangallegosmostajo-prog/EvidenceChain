namespace EvidenceChain.Application.CustodyTransfers.Accept;

public sealed record AcceptCustodyTransferCommand(
    Guid TransferId,
    Guid ActorId,
    byte[] ExpectedRowVersion);