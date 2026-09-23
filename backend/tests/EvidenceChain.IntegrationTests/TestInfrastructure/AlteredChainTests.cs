using System.Reflection;
using EvidenceChain.Application.Evidences.VerifyChain;
using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using EvidenceChain.IntegrationTests.TestInfrastructure;

namespace EvidenceChain.IntegrationTests;

public sealed class AlteredChainTests
{
    [Fact]
    public async Task VerifyChain_WhenEventWasAltered_ReturnsCompromised()
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

        var evidenceId =
            Guid.NewGuid();

        var custodianId =
            Guid.NewGuid();

        var actorId =
            Guid.NewGuid();

        var evidence =
            new Evidence(
                evidenceId,
                "EV-TEST-001",
                "Evidencia para probar alteración.",
                custodianId,
                now);

        var custodyEvent =
            CustodyEvent.Create(
                Guid.NewGuid(),
                evidenceId,
                1,
                CustodyEventType.EvidenceRegistered,
                actorId,
                null,
                custodianId,
                null,
                now,
                "Evidencia registrada correctamente.",
                CustodyEvent.GenesisPreviousHash);

        AlterEventDetails(
            custodyEvent,
            "Contenido alterado directamente en la base de datos.");

        dbContext.Evidences.Add(
            evidence);

        dbContext.CustodyEvents.Add(
            custodyEvent);

        await dbContext.SaveChangesAsync();

        var handler =
            new VerifyEvidenceChainHandler(
                dbContext,
                new FixedTimeProvider(
                    now.AddMinutes(10)));

        var result =
            await handler.HandleAsync(
                evidenceId);

        Assert.False(
            result.IsIntact);

        Assert.Equal(
            IntegrityStatus.Compromised.ToString(),
            result.IntegrityStatus);

        Assert.Equal(
            1,
            result.FirstInvalidSequenceNumber);

        Assert.Equal(
            custodyEvent.Id,
            result.FirstInvalidEventId);

        Assert.NotNull(
            result.FailureReason);

        Assert.NotEqual(
            result.StoredHash,
            result.CalculatedHash);
    }

    private static void AlterEventDetails(
        CustodyEvent custodyEvent,
        string alteredDetails)
    {
        var property =
            typeof(CustodyEvent).GetProperty(
                nameof(CustodyEvent.Details),
                BindingFlags.Instance |
                BindingFlags.Public);

        Assert.NotNull(property);

        property.SetValue(
            custodyEvent,
            alteredDetails);
    }
}