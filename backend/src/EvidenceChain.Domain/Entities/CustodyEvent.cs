using EvidenceChain.Domain.Enums;
using EvidenceChain.Domain.Services;

namespace EvidenceChain.Domain.Entities;

public sealed class CustodyEvent
{
    public const string GenesisPreviousHash =
        "0000000000000000000000000000000000000000000000000000000000000000";

    private CustodyEvent()
    {
    }

    private CustodyEvent(
        Guid id,
        Guid evidenceId,
        long sequenceNumber,
        CustodyEventType eventType,
        Guid actorId,
        Guid? fromCustodianId,
        Guid? toCustodianId,
        Guid? transferId,
        DateTimeOffset occurredAtUtc,
        string details,
        string previousHash)
    {
        Id = id;
        EvidenceId = evidenceId;
        SequenceNumber = sequenceNumber;
        EventType = eventType;
        ActorId = actorId;
        FromCustodianId = fromCustodianId;
        ToCustodianId = toCustodianId;
        TransferId = transferId;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        Details = details.Trim();
        PreviousHash = previousHash.ToLowerInvariant();

        Hash = CustodyEventHashCalculator.Calculate(
            Id,
            EvidenceId,
            SequenceNumber,
            EventType,
            ActorId,
            FromCustodianId,
            ToCustodianId,
            TransferId,
            OccurredAtUtc,
            Details,
            PreviousHash);
    }

    public Guid Id { get; private set; }

    public Guid EvidenceId { get; private set; }

    public long SequenceNumber { get; private set; }

    public CustodyEventType EventType { get; private set; }

    public Guid ActorId { get; private set; }

    public Guid? FromCustodianId { get; private set; }

    public Guid? ToCustodianId { get; private set; }

    public Guid? TransferId { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string Details { get; private set; } = string.Empty;

    public string PreviousHash { get; private set; } = string.Empty;

    public string Hash { get; private set; } = string.Empty;

    public static CustodyEvent Create(
        Guid id,
        Guid evidenceId,
        long sequenceNumber,
        CustodyEventType eventType,
        Guid actorId,
        Guid? fromCustodianId,
        Guid? toCustodianId,
        Guid? transferId,
        DateTimeOffset occurredAtUtc,
        string details,
        string previousHash)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Event ID is required.");

        if (evidenceId == Guid.Empty)
            throw new ArgumentException("Evidence ID is required.");

        if (actorId == Guid.Empty)
            throw new ArgumentException("Actor ID is required.");

        if (sequenceNumber < 1)
            throw new ArgumentOutOfRangeException(
                nameof(sequenceNumber));

        if (!Enum.IsDefined(eventType))
            throw new ArgumentOutOfRangeException(
                nameof(eventType));

        ArgumentException.ThrowIfNullOrWhiteSpace(details);
        ArgumentException.ThrowIfNullOrWhiteSpace(previousHash);

        if (details.Trim().Length > 1000)
        {
            throw new ArgumentException(
                "Event details cannot exceed 1000 characters.",
                nameof(details));
        }

        if (previousHash.Length != 64 ||
            !previousHash.All(Uri.IsHexDigit))
        {
            throw new ArgumentException(
                "Previous hash must contain 64 hexadecimal characters.",
                nameof(previousHash));
        }

        if (sequenceNumber == 1 &&
            !string.Equals(
                previousHash,
                GenesisPreviousHash,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The first event must use the genesis hash.",
                nameof(previousHash));
        }

        return new CustodyEvent(
            id,
            evidenceId,
            sequenceNumber,
            eventType,
            actorId,
            fromCustodianId,
            toCustodianId,
            transferId,
            occurredAtUtc,
            details,
            previousHash);
    }

    public string RecalculateHash()
    {
        return CustodyEventHashCalculator.Calculate(
            Id,
            EvidenceId,
            SequenceNumber,
            EventType,
            ActorId,
            FromCustodianId,
            ToCustodianId,
            TransferId,
            OccurredAtUtc,
            Details,
            PreviousHash);
    }
}