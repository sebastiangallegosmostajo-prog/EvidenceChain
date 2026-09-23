using EvidenceChain.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Evidences.List;

public sealed class GetEvidenceListHandler
{
    private readonly IEvidenceChainDbContext _dbContext;

    public GetEvidenceListHandler(
        IEvidenceChainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EvidenceListResult> HandleAsync(
        GetEvidenceListQuery request,
        CancellationToken cancellationToken = default)
    {
        if (request.Page < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Page),
                "La página debe ser mayor o igual que 1.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.PageSize),
                "El tamaño de página debe estar entre 1 y 100.");
        }

        var evidences =
            _dbContext.Evidences
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(
                request.Search))
        {
            var search =
                request.Search.Trim();

            evidences =
                evidences.Where(evidence =>
                    evidence.Code.Contains(search) ||
                    evidence.Description.Contains(search));
        }

        if (request.CustodianId.HasValue)
        {
            evidences =
                evidences.Where(evidence =>
                    evidence.CurrentCustodianId ==
                    request.CustodianId.Value);
        }

        if (request.IntegrityStatus.HasValue)
        {
            evidences =
                evidences.Where(evidence =>
                    evidence.IntegrityStatus ==
                    request.IntegrityStatus.Value);
        }

        var totalCount =
            await evidences.CountAsync(
                cancellationToken);

        evidences =
            string.Equals(
                request.SortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase)
                ? evidences
                    .OrderBy(evidence =>
                        evidence.LastEventAtUtc)
                    .ThenBy(evidence =>
                        evidence.Id)
                : evidences
                    .OrderByDescending(evidence =>
                        evidence.LastEventAtUtc)
                    .ThenBy(evidence =>
                        evidence.Id);

        var items =
            await evidences
                .Skip(
                    (request.Page - 1) *
                    request.PageSize)
                .Take(request.PageSize)
                .Select(evidence =>
                    new EvidenceListItem(
                        evidence.Id,
                        evidence.Code,
                        evidence.Description,
                        evidence.CurrentCustodianId,

                        _dbContext.Users
                            .Where(user =>
                                user.Id ==
                                evidence.CurrentCustodianId)
                            .Select(user =>
                                user.Name)
                            .FirstOrDefault()
                        ?? "Custodio desconocido",

                        evidence.CreatedAtUtc,
                        evidence.LastEventAtUtc,
                        evidence.IntegrityStatus
                            .ToString()))
                .ToListAsync(
                    cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        return new EvidenceListResult(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}