using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Infrastructure.Persistence.Seeding;

public sealed class DatabaseSeeder
{
    private readonly EvidenceChainDbContext _dbContext;
    private readonly IPasswordService _passwordService;

    public DatabaseSeeder(
        EvidenceChainDbContext dbContext,
        IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
    }

    public async Task SeedUsersAsync(
        string defaultPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            defaultPassword);

        var definitions = new[]
        {
            new UserDefinition(
                Guid.Parse(
                    "11111111-1111-1111-1111-111111111111"),
                "Investigador Demo",
                "investigador@evidencechain.local",
                UserRole.Investigator),

            new UserDefinition(
                Guid.Parse(
                    "22222222-2222-2222-2222-222222222222"),
                "Custodio Uno",
                "custodio1@evidencechain.local",
                UserRole.Custodian),

            new UserDefinition(
                Guid.Parse(
                    "33333333-3333-3333-3333-333333333333"),
                "Custodio Dos",
                "custodio2@evidencechain.local",
                UserRole.Custodian),

            new UserDefinition(
                Guid.Parse(
                    "44444444-4444-4444-4444-444444444444"),
                "Supervisor Demo",
                "supervisor@evidencechain.local",
                UserRole.Supervisor)
        };

        foreach (var definition in definitions)
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(
                    item => item.Email == definition.Email,
                    cancellationToken);

            if (user is null)
            {
                user = new UserAccount(
                    definition.Id,
                    definition.Name,
                    definition.Email,
                    definition.Role);

                var passwordHash =
                    _passwordService.HashPassword(
                        user,
                        defaultPassword);

                user.SetPasswordHash(passwordHash);

                _dbContext.Users.Add(user);

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    user.PasswordHash))
            {
                var passwordHash =
                    _passwordService.HashPassword(
                        user,
                        defaultPassword);

                user.SetPasswordHash(passwordHash);
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private sealed record UserDefinition(
        Guid Id,
        string Name,
        string Email,
        UserRole Role);
}