using EvidenceChain.Application.Evidences.Chain;
using EvidenceChain.Application.Evidences.Detail;
using EvidenceChain.Application.Evidences.List;
using EvidenceChain.Application.Evidences.VerifyChain;
using EvidenceChain.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceChain.Api.Controllers;

[ApiController]
[Route("api/v1/evidence")]
[Authorize]
public sealed class EvidenceController
    : ControllerBase
{
    private readonly GetEvidenceListHandler
        _getEvidenceListHandler;

    private readonly GetEvidenceDetailHandler
        _getEvidenceDetailHandler;

    private readonly GetEvidenceChainHandler
        _getEvidenceChainHandler;

    private readonly VerifyEvidenceChainHandler
        _verifyEvidenceChainHandler;

    public EvidenceController(
        GetEvidenceListHandler getEvidenceListHandler,
        GetEvidenceDetailHandler getEvidenceDetailHandler,
        GetEvidenceChainHandler getEvidenceChainHandler,
        VerifyEvidenceChainHandler verifyEvidenceChainHandler)
    {
        _getEvidenceListHandler =
            getEvidenceListHandler;

        _getEvidenceDetailHandler =
            getEvidenceDetailHandler;

        _getEvidenceChainHandler =
            getEvidenceChainHandler;

        _verifyEvidenceChainHandler =
            verifyEvidenceChainHandler;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(EvidenceListResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EvidenceListResult>>
        GetEvidenceList(
            [FromQuery]
            string? search,

            [FromQuery]
            Guid? custodianId,

            [FromQuery]
            IntegrityStatus? integrityStatus,

            [FromQuery]
            string sortDirection = "desc",

            [FromQuery]
            int page = 1,

            [FromQuery]
            int pageSize = 20,

            CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "Página inválida.",
                detail:
                    "El número de página debe ser mayor o igual que 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "Tamaño de página inválido.",
                detail:
                    "El tamaño de página debe estar entre 1 y 100.");
        }

        if (!string.Equals(
                sortDirection,
                "asc",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "Ordenamiento inválido.",
                detail:
                    "sortDirection solamente admite asc o desc.");
        }

        var query =
            new GetEvidenceListQuery(
                search,
                custodianId,
                integrityStatus,
                sortDirection,
                page,
                pageSize);

        var result =
            await _getEvidenceListHandler
                .HandleAsync(
                    query,
                    cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(EvidenceDetailResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDetailResult>>
        GetEvidenceDetail(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _getEvidenceDetailHandler
                .HandleAsync(
                    id,
                    cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/chain")]
    [ProducesResponseType(
        typeof(EvidenceChainResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceChainResult>>
        GetEvidenceChain(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _getEvidenceChainHandler
                .HandleAsync(
                    id,
                    cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/chain/verify")]
    [ProducesResponseType(
        typeof(ChainVerificationResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChainVerificationResult>>
        VerifyEvidenceChain(
            Guid id,
            CancellationToken cancellationToken)
    {
        var result =
            await _verifyEvidenceChainHandler
                .HandleAsync(
                    id,
                    cancellationToken);

        return Ok(result);
    }
}