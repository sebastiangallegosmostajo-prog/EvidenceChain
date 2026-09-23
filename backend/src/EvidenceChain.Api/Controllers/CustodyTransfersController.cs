using System.Security.Claims;
using EvidenceChain.Api.Contracts.CustodyTransfers;
using EvidenceChain.Application.CustodyTransfers.Accept;
using EvidenceChain.Application.CustodyTransfers.Common;
using EvidenceChain.Application.CustodyTransfers.Reject;
using EvidenceChain.Application.CustodyTransfers.Request;
using EvidenceChain.Application.CustodyTransfers.ListPending;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EvidenceChain.Api.Controllers;

[ApiController]
[Route("api/v1/custody-transfers")]
[Authorize]
public sealed class CustodyTransfersController
    : ControllerBase
{
    private readonly RequestCustodyTransferHandler
        _requestTransferHandler;

    private readonly AcceptCustodyTransferHandler
        _acceptTransferHandler;

    private readonly RejectCustodyTransferHandler
        _rejectTransferHandler;

    private readonly GetPendingCustodyTransfersHandler
        _getPendingTransfersHandler;

    public CustodyTransfersController(
        RequestCustodyTransferHandler requestTransferHandler,
        AcceptCustodyTransferHandler acceptTransferHandler,
        RejectCustodyTransferHandler rejectTransferHandler,
        GetPendingCustodyTransfersHandler getPendingTransfersHandler)
    {
        _requestTransferHandler =
            requestTransferHandler;

        _acceptTransferHandler =
            acceptTransferHandler;

        _rejectTransferHandler =
            rejectTransferHandler;

        _getPendingTransfersHandler =
            getPendingTransfersHandler;
    }

    // ==========================================
    // Consultar transferencias pendientes
    // ==========================================

    [HttpGet("pending")]
    [Authorize(Roles = "Custodian")]
    [ProducesResponseType(
        typeof(PendingCustodyTransfersResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<PendingCustodyTransfersResult>>
        GetPendingTransfers(
            CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var custodianId))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status401Unauthorized,
                title:
                    "Token inválido.",
                detail:
                    "El token no contiene un identificador de usuario válido.");
        }

        var result =
            await _getPendingTransfersHandler
                .HandleAsync(
                    custodianId,
                    cancellationToken);

        return Ok(result);
    }

    // ==========================================
    // Solicitar una transferencia
    // ==========================================

    [HttpPost]
    [Authorize(Roles = "Investigator,Supervisor")]
    [ProducesResponseType(
        typeof(RequestCustodyTransferResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<RequestCustodyTransferResult>>
        RequestTransfer(
            [FromBody]
            RequestCustodyTransferRequest request,

            [FromHeader(Name = "Idempotency-Key")]
            string? idempotencyKey,

            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
                idempotencyKey))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "Idempotency-Key obligatoria.",
                detail:
                    "Debe enviar la cabecera Idempotency-Key.");
        }

        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var requestedById))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status401Unauthorized,
                title:
                    "Token inválido.",
                detail:
                    "El token no contiene un identificador de usuario válido.");
        }

        var command =
            new RequestCustodyTransferCommand(
                request.EvidenceId,
                request.ToCustodianId,
                requestedById,
                idempotencyKey);

        var result =
            await _requestTransferHandler
                .HandleAsync(
                    command,
                    cancellationToken);

        Response.Headers.ETag =
            $"\"{result.RowVersion}\"";

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    // ==========================================
    // Aceptar una transferencia
    // ==========================================

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Custodian")]
    [ProducesResponseType(
        typeof(CustodyTransferActionResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<CustodyTransferActionResult>>
        AcceptTransfer(
            Guid id,

            [FromHeader(Name = "If-Match")]
            string? ifMatch,

            CancellationToken cancellationToken)
    {
        if (!TryParseETag(
                ifMatch,
                out var expectedRowVersion))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "If-Match inválido.",
                detail:
                    "Debe enviar el ETag vigente en la cabecera If-Match.");
        }

        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var actorId))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status401Unauthorized,
                title:
                    "Token inválido.",
                detail:
                    "El token no contiene un identificador de usuario válido.");
        }

        var command =
            new AcceptCustodyTransferCommand(
                id,
                actorId,
                expectedRowVersion);

        var result =
            await _acceptTransferHandler
                .HandleAsync(
                    command,
                    cancellationToken);

        Response.Headers.ETag =
            $"\"{result.RowVersion}\"";

        return Ok(result);
    }

    // ==========================================
    // Rechazar una transferencia
    // ==========================================

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Custodian")]
    [ProducesResponseType(
        typeof(CustodyTransferActionResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<
        ActionResult<CustodyTransferActionResult>>
        RejectTransfer(
            Guid id,

            [FromBody]
            RejectCustodyTransferRequest request,

            [FromHeader(Name = "If-Match")]
            string? ifMatch,

            CancellationToken cancellationToken)
    {
        if (!TryParseETag(
                ifMatch,
                out var expectedRowVersion))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "If-Match inválido.",
                detail:
                    "Debe enviar el ETag vigente en la cabecera If-Match.");
        }

        if (string.IsNullOrWhiteSpace(
                request.Reason))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status400BadRequest,
                title:
                    "Motivo obligatorio.",
                detail:
                    "Debe indicar el motivo del rechazo.");
        }

        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var actorId))
        {
            return Problem(
                statusCode:
                    StatusCodes.Status401Unauthorized,
                title:
                    "Token inválido.",
                detail:
                    "El token no contiene un identificador de usuario válido.");
        }

        var command =
            new RejectCustodyTransferCommand(
                id,
                actorId,
                request.Reason,
                expectedRowVersion);

        var result =
            await _rejectTransferHandler
                .HandleAsync(
                    command,
                    cancellationToken);

        Response.Headers.ETag =
            $"\"{result.RowVersion}\"";

        return Ok(result);
    }

    // ==========================================
    // Conversión de ETag a rowversion
    // ==========================================

    private static bool TryParseETag(
        string? etag,
        out byte[] rowVersion)
    {
        rowVersion =
            Array.Empty<byte>();

        if (string.IsNullOrWhiteSpace(
                etag))
        {
            return false;
        }

        var value =
            etag.Trim();

        if (value.StartsWith(
                "W/",
                StringComparison.OrdinalIgnoreCase))
        {
            value =
                value[2..].Trim();
        }

        if (value.Length >= 2 &&
            value.StartsWith('"') &&
            value.EndsWith('"'))
        {
            value =
                value[1..^1];
        }

        try
        {
            rowVersion =
                Convert.FromBase64String(
                    value);

            return rowVersion.Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}