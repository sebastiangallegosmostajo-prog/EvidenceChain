using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Evidences.Detail;

public sealed class GetEvidenceDetailHandler
{
    private readonly IEvidenceChainDbContext _dbContext;

    public GetEvidenceDetailHandler(
        IEvidenceChainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EvidenceDetailResult> HandleAsync(
        Guid evidenceId,
        CancellationToken cancellationToken = default)
    {
        if (evidenceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador de evidencia es obligatorio.",
                nameof(evidenceId));
        }

        var result =
            await _dbContext.Evidences
                .AsNoTracking()
                .Where(evidence =>
                    evidence.Id == evidenceId)
                .Select(evidence =>
                    new EvidenceDetailResult(
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

                        _dbContext.Users
                            .Where(user =>
                                user.Id ==
                                evidence.CurrentCustodianId)
                            .Select(user =>
                                user.Email)
                            .FirstOrDefault()
                        ?? string.Empty,

                        evidence.CreatedAtUtc,
                        evidence.LastEventAtUtc,
                        evidence.IntegrityStatus
                            .ToString()))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (result is null)
        {
            throw new NotFoundException(
                $"No se encontró la evidencia {evidenceId}.");
        }

        return result;
    }
}