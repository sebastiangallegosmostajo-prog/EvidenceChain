namespace EvidenceChain.Application.Common.Models;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord()
    {
    }

    public Guid Id { get; private set; }

    public string Key { get; private set; } = string.Empty;

    public string Operation { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public Guid ResourceId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static IdempotencyRecord Create(
        string key,
        string operation,
        string requestHash,
        Guid resourceId,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestHash);

        if (key.Trim().Length > 100)
        {
            throw new ArgumentException(
                "La clave de idempotencia no puede superar 100 caracteres.",
                nameof(key));
        }

        if (operation.Trim().Length > 100)
        {
            throw new ArgumentException(
                "La operación no puede superar 100 caracteres.",
                nameof(operation));
        }

        if (requestHash.Length != 64 ||
            !requestHash.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "El hash de la solicitud debe contener 64 caracteres hexadecimales.",
                nameof(requestHash));
        }

        if (resourceId == Guid.Empty)
        {
            throw new ArgumentException(
                "El recurso asociado es obligatorio.",
                nameof(resourceId));
        }

        return new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            Operation = operation.Trim(),
            RequestHash = requestHash.ToLowerInvariant(),
            ResourceId = resourceId,
            CreatedAtUtc = createdAtUtc.ToUniversalTime()
        };
    }
}