namespace EvidenceChain.Application.Evidences.Detail;

public sealed record EvidenceDetailResult(
    Guid Id,
    string Code,
    string Description,
    Guid CurrentCustodianId,
    string CurrentCustodianName,
    string CurrentCustodianEmail,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastEventAtUtc,
    string IntegrityStatus);