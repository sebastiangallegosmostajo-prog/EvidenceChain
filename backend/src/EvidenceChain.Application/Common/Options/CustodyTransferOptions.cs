namespace EvidenceChain.Application.Common.Options;

public sealed class CustodyTransferOptions
{
    public const string SectionName = "CustodyTransfers";

    public int PendingExpirationHours { get; init; } = 24;
}