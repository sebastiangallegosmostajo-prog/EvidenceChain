using EvidenceChain.Domain.Enums;

namespace EvidenceChain.Domain.Entities;

public sealed class CustodyTransfer
{
    private CustodyTransfer()
    {
    }

    public Guid Id { get; private set; }

    public Guid EvidenceId { get; private set; }

    public Guid FromCustodianId { get; private set; }

    public Guid ToCustodianId { get; private set; }

    public Guid RequestedById { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTimeOffset RequestedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RespondedAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static CustodyTransfer Request(
        Guid evidenceId,
        Guid fromCustodianId,
        Guid toCustodianId,
        Guid requestedById,
        DateTimeOffset requestedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (evidenceId == Guid.Empty)
        {
            throw new ArgumentException(
                "La evidencia es obligatoria.",
                nameof(evidenceId));
        }

        if (fromCustodianId == Guid.Empty)
        {
            throw new ArgumentException(
                "El custodio actual es obligatorio.",
                nameof(fromCustodianId));
        }

        if (toCustodianId == Guid.Empty)
        {
            throw new ArgumentException(
                "El nuevo custodio es obligatorio.",
                nameof(toCustodianId));
        }

        if (requestedById == Guid.Empty)
        {
            throw new ArgumentException(
                "El usuario solicitante es obligatorio.",
                nameof(requestedById));
        }

        if (fromCustodianId == toCustodianId)
        {
            throw new ArgumentException(
                "La evidencia no puede transferirse al mismo custodio.");
        }

        if (expiresAtUtc <= requestedAtUtc)
        {
            throw new ArgumentException(
                "La fecha de vencimiento debe ser posterior a la solicitud.");
        }

        return new CustodyTransfer
        {
            Id = Guid.NewGuid(),
            EvidenceId = evidenceId,
            FromCustodianId = fromCustodianId,
            ToCustodianId = toCustodianId,
            RequestedById = requestedById,
            Status = TransferStatus.Pending,
            RequestedAtUtc = requestedAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public void Accept(
        Guid actorId,
        DateTimeOffset respondedAtUtc)
    {
        EnsureIsPending();

        if (actorId != ToCustodianId)
        {
            throw new InvalidOperationException(
                "Solo el custodio destinatario puede aceptar la transferencia.");
        }

        ValidateResponseDate(respondedAtUtc);

        Status = TransferStatus.Accepted;
        RespondedAtUtc = respondedAtUtc;
        RejectionReason = null;
    }

    public void Reject(
        Guid actorId,
        DateTimeOffset respondedAtUtc,
        string reason)
    {
        EnsureIsPending();

        if (actorId != ToCustodianId)
        {
            throw new InvalidOperationException(
                "Solo el custodio destinatario puede rechazar la transferencia.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Debe indicar el motivo del rechazo.",
                nameof(reason));
        }

        ValidateResponseDate(respondedAtUtc);

        Status = TransferStatus.Rejected;
        RespondedAtUtc = respondedAtUtc;
        RejectionReason = reason.Trim();
    }

    private void EnsureIsPending()
    {
        if (Status != TransferStatus.Pending)
        {
            throw new InvalidOperationException(
                $"La transferencia ya se encuentra en estado {Status}.");
        }
    }

    private void ValidateResponseDate(DateTimeOffset respondedAtUtc)
    {
        if (respondedAtUtc < RequestedAtUtc)
        {
            throw new ArgumentException(
                "La respuesta no puede ser anterior a la solicitud.",
                nameof(respondedAtUtc));
        }
    }
}