using EvidenceChain.Domain.Entities;

namespace EvidenceChain.Application.Common.Interfaces;

public interface IPasswordService
{
    string HashPassword(
        UserAccount user,
        string password);

    bool VerifyPassword(
        UserAccount user,
        string passwordHash,
        string providedPassword);
}