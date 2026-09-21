using EvidenceChain.Domain.Entities;

namespace EvidenceChain.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserAccount user);
}