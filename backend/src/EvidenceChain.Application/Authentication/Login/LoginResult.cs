namespace EvidenceChain.Application.Authentication.Login;

public sealed record LoginResult(
    string AccessToken,
    string TokenType,
    Guid UserId,
    string Name,
    string Email,
    string Role);