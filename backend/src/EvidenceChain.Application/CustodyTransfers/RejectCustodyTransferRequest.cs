namespace EvidenceChain.Api.Contracts.CustodyTransfers;

public sealed record RejectCustodyTransferRequest(
    string Reason);