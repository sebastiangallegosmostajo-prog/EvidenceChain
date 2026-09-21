using System.ComponentModel.DataAnnotations;

namespace EvidenceChain.Api.Contracts.Authentication;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } =
        string.Empty;

    [Required]
    public string Password { get; init; } =
        string.Empty;
}