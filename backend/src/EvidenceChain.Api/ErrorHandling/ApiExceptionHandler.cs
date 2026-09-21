using EvidenceChain.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Api.ErrorHandling;

public sealed class ApiExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = exception switch
        {
            ArgumentException => new ErrorDefinition(
                StatusCodes.Status400BadRequest,
                "Solicitud inválida.",
                exception.Message),

            NotFoundException => new ErrorDefinition(
                StatusCodes.Status404NotFound,
                "Recurso no encontrado.",
                exception.Message),

            ConcurrencyConflictException => new ErrorDefinition(
                StatusCodes.Status409Conflict,
                "Conflicto de concurrencia.",
                exception.Message),

            ConflictException => new ErrorDefinition(
                StatusCodes.Status409Conflict,
                "Conflicto con el estado actual.",
                exception.Message),

            DbUpdateConcurrencyException => new ErrorDefinition(
                StatusCodes.Status409Conflict,
                "Conflicto de concurrencia.",
                "El recurso fue modificado por otro usuario."),

            _ => new ErrorDefinition(
                StatusCodes.Status500InternalServerError,
                "Error interno.",
                "Ocurrió un error inesperado.")
        };

        if (error.StatusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Error no controlado. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Solicitud rechazada con estado {StatusCode}. TraceId: {TraceId}",
                error.StatusCode,
                httpContext.TraceIdentifier);
        }

        var problem = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        if (exception is ConcurrencyConflictException
            concurrencyException)
        {
            problem.Extensions["currentState"] =
                concurrencyException.CurrentState;
        }

        httpContext.Response.StatusCode =
            error.StatusCode;

        httpContext.Response.ContentType =
            "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }

    private sealed record ErrorDefinition(
        int StatusCode,
        string Title,
        string Detail);
}