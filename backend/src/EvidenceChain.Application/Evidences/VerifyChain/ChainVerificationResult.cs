namespace EvidenceChain.Application.Evidences.VerifyChain;

public sealed record ChainVerificationResult(
    Guid EvidenceId,
    bool IsIntact,
    string IntegrityStatus,
    int EventCount,
    Guid? FirstInvalidEventId,
    long? FirstInvalidSequenceNumber,
    string? FailureReason,
    string? ExpectedPreviousHash,
    string? ActualPreviousHash,
    string? StoredHash,
    string? CalculatedHash,
    DateTimeOffset VerifiedAtUtc);