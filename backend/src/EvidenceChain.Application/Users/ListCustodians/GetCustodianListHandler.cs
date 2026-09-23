using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application
    .Users.ListCustodians;

public sealed class GetCustodianListHandler
{
    private readonly IEvidenceChainDbContext
        _dbContext;

    public GetCustodianListHandler(
        IEvidenceChainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<
        IReadOnlyList<CustodianListItem>>
        HandleAsync(
            CancellationToken cancellationToken =
                default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role ==
                    UserRole.Custodian)
            .OrderBy(user =>
                user.Name)
            .Select(user =>
                new CustodianListItem(
                    user.Id,
                    user.Name,
                    user.Email))
            .ToListAsync(
                cancellationToken);
    }
}