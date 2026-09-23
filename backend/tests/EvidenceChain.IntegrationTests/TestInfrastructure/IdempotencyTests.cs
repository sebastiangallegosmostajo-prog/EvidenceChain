using EvidenceChain.Application.Common.Options;
using EvidenceChain.Application.CustodyTransfers.Request;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using EvidenceChain.IntegrationTests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EvidenceChain.IntegrationTests;

public sealed class IdempotencyTests
{
    [Fact]
    public async Task RequestTransfer_WithSameKeyAndPayload_CreatesOneTransfer()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        var now =
            new DateTimeOffset(
                2026,
                9,
                23,
                12,
                0,
                0,
                TimeSpan.Zero);

        var currentCustodianId =
            Guid.NewGuid();

        var destinationCustodianId =
            Guid.NewGuid();

        var investigatorId =
            Guid.NewGuid();

        var evidenceId =
            Guid.NewGuid();

        dbContext.Users.AddRange(
            new UserAccount(
                currentCustodianId,
                "Custodio actual",
                "actual@test.local",
                UserRole.Custodian),

            new UserAccount(
                destinationCustodianId,
                "Custodio destino",
                "destino@test.local",
                UserRole.Custodian),

            new UserAccount(
                investigatorId,
                "Investigador",
                "investigador@test.local",
                UserRole.Investigator));

        dbContext.Evidences.Add(
            new Evidence(
                evidenceId,
                "EV-IDEMPOTENCY-001",
                "Evidencia para probar idempotencia.",
                currentCustodianId,
                now.AddMinutes(-10)));

        await dbContext.SaveChangesAsync();

        var options =
            Options.Create(
                new CustodyTransferOptions
                {
                    PendingExpirationHours = 24
                });

        var handler =
            new RequestCustodyTransferHandler(
                dbContext,
                new FixedTimeProvider(now),
                options);

        var command =
            new RequestCustodyTransferCommand(
                evidenceId,
                destinationCustodianId,
                investigatorId,
                "idempotency-test-001");

        var firstResult =
            await handler.HandleAsync(
                command);

        var secondResult =
            await handler.HandleAsync(
                command);

        Assert.Equal(
            firstResult.TransferId,
            secondResult.TransferId);

        Assert.Equal(
            1,
            await dbContext.CustodyTransfers.CountAsync());

        Assert.Equal(
            1,
            await dbContext.IdempotencyRecords.CountAsync());

        Assert.Equal(
            1,
            await dbContext.CustodyEvents.CountAsync());
    }
}