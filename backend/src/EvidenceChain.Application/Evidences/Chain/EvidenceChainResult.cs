namespace EvidenceChain.Application.Evidences.Chain;

public sealed record EvidenceChainEventItem(
    Guid Id,
    long SequenceNumber,
    string EventType,
    Guid ActorId,
    string ActorName,
    Guid? FromCustodianId,
    string? FromCustodianName,
    Guid? ToCustodianId,
    string? ToCustodianName,
    Guid? TransferId,
    DateTimeOffset OccurredAtUtc,
    string Details,
    string PreviousHash,
    string Hash,
    bool HasAnomaly,
    string? AnomalySeverity,
    string? AnomalyExplanation);

public sealed record EvidenceAnomalyItem(
    string Type,
    string Severity,
    string Explanation,
    Guid TransferId,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset ExpiresAtUtc);

public sealed record EvidenceChainResult(
    Guid EvidenceId,
    string EvidenceCode,
    string IntegrityStatus,
    IReadOnlyList<EvidenceChainEventItem> Events,
    IReadOnlyList<EvidenceAnomalyItem> Anomalies);