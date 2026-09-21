using System.ComponentModel.DataAnnotations;

namespace EvidenceChain.Api.Contracts.CustodyTransfers;

public sealed class RequestCustodyTransferRequest
{
    [Required]
    public Guid EvidenceId { get; init; }

    [Required]
    public Guid ToCustodianId { get; init; }
}