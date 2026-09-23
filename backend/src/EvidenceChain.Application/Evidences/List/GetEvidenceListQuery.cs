using EvidenceChain.Domain.Enums;

namespace EvidenceChain.Application.Evidences.List;

public sealed record GetEvidenceListQuery(
    string? Search,
    Guid? CustodianId,
    IntegrityStatus? IntegrityStatus,
    string SortDirection,
    int Page,
    int PageSize);