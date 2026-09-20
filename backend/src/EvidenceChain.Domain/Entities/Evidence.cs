using EvidenceChain.Domain.Enums;

namespace EvidenceChain.Domain.Entities;

public sealed class Evidence
{
    private Evidence()
    {
    }

    public Evidence(
        Guid id,
        string code,
        string description,
        Guid currentCustodianId,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (currentCustodianId == Guid.Empty)
        {
            throw new ArgumentException(
                "A current custodian is required.",
                nameof(currentCustodianId));
        }

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Description = description.Trim();
        CurrentCustodianId = currentCustodianId;
        CreatedAtUtc = createdAtUtc;
        LastEventAtUtc = createdAtUtc;
        IntegrityStatus = IntegrityStatus.Unknown;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Guid CurrentCustodianId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset LastEventAtUtc { get; private set; }

    public IntegrityStatus IntegrityStatus { get; private set; }
}