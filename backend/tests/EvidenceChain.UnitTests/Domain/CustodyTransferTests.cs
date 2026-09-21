using EvidenceChain.Domain.Entities;
using EvidenceChain.Domain.Enums;

namespace EvidenceChain.UnitTests.Domain;

public sealed class CustodyTransferTests
{
    [Fact]
    public void Request_ShouldCreatePendingTransfer()
    {
        var transfer = CreatePendingTransfer();

        Assert.Equal(TransferStatus.Pending, transfer.Status);
        Assert.Null(transfer.RespondedAtUtc);
        Assert.Null(transfer.RejectionReason);
    }

    [Fact]
    public void Accept_ShouldChangeStatusToAccepted()
    {
        var toCustodianId = Guid.NewGuid();
        var requestedAtUtc = DateTimeOffset.UtcNow;

        var transfer = CustodyTransfer.Request(
            Guid.NewGuid(),
            Guid.NewGuid(),
            toCustodianId,
            Guid.NewGuid(),
            requestedAtUtc,
            requestedAtUtc.AddHours(24));

        transfer.Accept(
            toCustodianId,
            requestedAtUtc.AddMinutes(10));

        Assert.Equal(TransferStatus.Accepted, transfer.Status);
        Assert.NotNull(transfer.RespondedAtUtc);
    }

    [Fact]
    public void Accept_ShouldFail_WhenActorIsNotRecipient()
    {
        var transfer = CreatePendingTransfer();

        var action = () => transfer.Accept(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(10));

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Reject_ShouldStoreReason()
    {
        var toCustodianId = Guid.NewGuid();
        var requestedAtUtc = DateTimeOffset.UtcNow;

        var transfer = CustodyTransfer.Request(
            Guid.NewGuid(),
            Guid.NewGuid(),
            toCustodianId,
            Guid.NewGuid(),
            requestedAtUtc,
            requestedAtUtc.AddHours(24));

        transfer.Reject(
            toCustodianId,
            requestedAtUtc.AddMinutes(10),
            "No tengo acceso al repositorio.");

        Assert.Equal(TransferStatus.Rejected, transfer.Status);
        Assert.Equal(
            "No tengo acceso al repositorio.",
            transfer.RejectionReason);
    }

    [Fact]
    public void Reject_ShouldFail_WhenTransferWasAlreadyAccepted()
    {
        var toCustodianId = Guid.NewGuid();
        var requestedAtUtc = DateTimeOffset.UtcNow;

        var transfer = CustodyTransfer.Request(
            Guid.NewGuid(),
            Guid.NewGuid(),
            toCustodianId,
            Guid.NewGuid(),
            requestedAtUtc,
            requestedAtUtc.AddHours(24));

        transfer.Accept(
            toCustodianId,
            requestedAtUtc.AddMinutes(10));

        var action = () => transfer.Reject(
            toCustodianId,
            requestedAtUtc.AddMinutes(20),
            "Intento de rechazo posterior.");

        Assert.Throws<InvalidOperationException>(action);
    }

    private static CustodyTransfer CreatePendingTransfer()
    {
        var requestedAtUtc = DateTimeOffset.UtcNow;

        return CustodyTransfer.Request(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            requestedAtUtc,
            requestedAtUtc.AddHours(24));
    }
}