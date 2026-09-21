using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EvidenceChain.Api.Authentication;

public sealed class JwtTokenGenerator
    : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(
        IOptions<JwtOptions> options,
        TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public string GenerateToken(UserAccount user)
    {
        ValidateOptions();

        var now = _timeProvider.GetUtcNow();

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.Name),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now
                .AddMinutes(_options.ExpirationMinutes)
                .UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.Issuer))
        {
            throw new InvalidOperationException(
                "La configuración Jwt:Issuer es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException(
                "La configuración Jwt:Audience es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(_options.Key) ||
            _options.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key debe contener al menos 32 caracteres.");
        }

        if (_options.ExpirationMinutes is < 5 or > 1440)
        {
            throw new InvalidOperationException(
                "Jwt:ExpirationMinutes debe estar entre 5 y 1440.");
        }
    }
}