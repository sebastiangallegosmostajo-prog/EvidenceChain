namespace EvidenceChain.Application
    .Users.ListCustodians;

public sealed record CustodianListItem(
    Guid Id,
    string Name,
    string Email);