using EvidenceChain.Domain.Enums;

namespace EvidenceChain.Domain.Entities;

public sealed class UserAccount
{
    private UserAccount()
    {
    }

    public UserAccount(
        Guid id,
        string name,
        string email,
        UserRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Id = id;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }
}