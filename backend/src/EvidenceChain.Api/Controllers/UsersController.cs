using EvidenceChain.Application
    .Users.ListCustodians;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceChain.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public sealed class UsersController
    : ControllerBase
{
    private readonly
        GetCustodianListHandler
        _getCustodianListHandler;

    public UsersController(
        GetCustodianListHandler
            getCustodianListHandler)
    {
        _getCustodianListHandler =
            getCustodianListHandler;
    }

    [HttpGet("custodians")]
    [Authorize(
        Roles = "Investigator,Supervisor")]
    [ProducesResponseType(
        typeof(
            IReadOnlyList<
                CustodianListItem>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<
            IReadOnlyList<
                CustodianListItem>>>
        GetCustodians(
            CancellationToken cancellationToken)
    {
        var result =
            await _getCustodianListHandler
                .HandleAsync(
                    cancellationToken);

        return Ok(result);
    }
}