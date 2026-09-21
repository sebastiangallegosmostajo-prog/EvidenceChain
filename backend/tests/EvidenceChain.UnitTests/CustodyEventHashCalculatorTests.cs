using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;
using EvidenceChain.Domain.Services;

namespace EvidenceChain.UnitTests;

public sealed class CustodyEventHashCalculatorTests
{
    [Fact]
    public void Calculate_WithSameCanonicalData_ReturnsSameHash()
    {
        var eventId =
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var evidenceId =
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var actorId =
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var occurredAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                15,
                30,
                0,
                TimeSpan.Zero);

        var firstHash = CustodyEventHashCalculator.Calculate(
            eventId,
            evidenceId,
            1,
            CustodyEventType.EvidenceRegistered,
            actorId,
            null,
            actorId,
            null,
            occurredAtUtc,
            "Evidence registered",
            CustodyEvent.GenesisPreviousHash);

        var secondHash = CustodyEventHashCalculator.Calculate(
            eventId,
            evidenceId,
            1,
            CustodyEventType.EvidenceRegistered,
            actorId,
            null,
            actorId,
            null,
            occurredAtUtc,
            "Evidence registered",
            CustodyEvent.GenesisPreviousHash);

        Assert.Equal(firstHash, secondHash);
        Assert.Equal(64, firstHash.Length);
    }

    [Fact]
    public void Calculate_WhenDetailsChange_ReturnsDifferentHash()
    {
        var eventId =
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var evidenceId =
            Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var actorId =
            Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var occurredAtUtc =
            new DateTimeOffset(
                2026,
                9,
                20,
                15,
                30,
                0,
                TimeSpan.Zero);

        var originalHash = CustodyEventHashCalculator.Calculate(
            eventId,
            evidenceId,
            1,
            CustodyEventType.EvidenceRegistered,
            actorId,
            null,
            actorId,
            null,
            occurredAtUtc,
            "Evidence registered",
            CustodyEvent.GenesisPreviousHash);

        var alteredHash = CustodyEventHashCalculator.Calculate(
            eventId,
            evidenceId,
            1,
            CustodyEventType.EvidenceRegistered,
            actorId,
            null,
            actorId,
            null,
            occurredAtUtc,
            "Evidence was modified",
            CustodyEvent.GenesisPreviousHash);

        Assert.NotEqual(originalHash, alteredHash);
    }
}