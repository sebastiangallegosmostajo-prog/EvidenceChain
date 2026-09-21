using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EvidenceChain.Api.Authentication;

public sealed class AspNetPasswordService
    : IPasswordService
{
    private readonly PasswordHasher<UserAccount> _hasher =
        new();

    public string HashPassword(
        UserAccount user,
        string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            password);

        return _hasher.HashPassword(
            user,
            password);
    }

    public bool VerifyPassword(
        UserAccount user,
        string passwordHash,
        string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrWhiteSpace(providedPassword))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(
            user,
            passwordHash,
            providedPassword);

        return result != PasswordVerificationResult.Failed;
    }
}