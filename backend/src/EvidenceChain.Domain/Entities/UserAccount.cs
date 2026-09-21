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

        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del usuario es obligatorio.",
                nameof(id));
        }

        Id = id;
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            passwordHash);

        if (passwordHash.Length > 500)
        {
            throw new ArgumentException(
                "El hash de contraseña no puede superar 500 caracteres.",
                nameof(passwordHash));
        }

        PasswordHash = passwordHash;
    }
}