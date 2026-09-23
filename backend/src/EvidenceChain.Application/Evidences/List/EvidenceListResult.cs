namespace EvidenceChain.Application.Evidences.List;

public sealed record EvidenceListItem(
    Guid Id,
    string Code,
    string Description,
    Guid CurrentCustodianId,
    string CurrentCustodianName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastEventAtUtc,
    string IntegrityStatus);

public sealed record EvidenceListResult(
    IReadOnlyList<EvidenceListItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);