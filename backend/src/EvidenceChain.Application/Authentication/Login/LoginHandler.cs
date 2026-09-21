using EvidenceChain.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.Application.Authentication.Login;

public sealed class LoginHandler
{
    private readonly IEvidenceChainDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginHandler(
        IEvidenceChainDbContext dbContext,
        IPasswordService passwordService,
        IJwtTokenGenerator tokenGenerator)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResult?> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) ||
            string.IsNullOrWhiteSpace(command.Password))
        {
            return null;
        }

        var normalizedEmail =
            command.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Email == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var passwordIsValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                command.Password);

        if (!passwordIsValid)
        {
            return null;
        }

        var accessToken =
            _tokenGenerator.GenerateToken(user);

        return new LoginResult(
            accessToken,
            "Bearer",
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString());
    }
}