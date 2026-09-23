using System.Reflection;
using EvidenceChain.Application.Common.Exceptions;
using EvidenceChain.Application.CustodyTransfers.Accept;
using EvidenceChain.Domain.Entities;
using EvidenceChain.IntegrationTests.TestInfrastructure;

namespace EvidenceChain.IntegrationTests;

public sealed class ConcurrencyConflictTests
{
    [Fact]
    public async Task AcceptTransfer_WithStaleRowVersion_ThrowsConflict()
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

        var destinationCustodianId =
            Guid.NewGuid();

        var transfer =
            CustodyTransfer.Request(
                Guid.NewGuid(),
                Guid.NewGuid(),
                destinationCustodianId,
                Guid.NewGuid(),
                now.AddMinutes(-5),
                now.AddHours(24));

        dbContext.CustodyTransfers.Add(
            transfer);

        await dbContext.SaveChangesAsync();

        SetRowVersion(
            transfer,
            new byte[]
            {
                1, 2, 3, 4,
                5, 6, 7, 8
            });

        var handler =
            new AcceptCustodyTransferHandler(
                dbContext,
                new FixedTimeProvider(now));

        var staleRowVersion =
            new byte[]
            {
                9, 9, 9, 9,
                9, 9, 9, 9
            };

        var command =
            new AcceptCustodyTransferCommand(
                transfer.Id,
                destinationCustodianId,
                staleRowVersion);

        var exception =
            await Assert.ThrowsAsync<
                ConcurrencyConflictException>(
                () => handler.HandleAsync(command));

        Assert.Contains(
            "modificada",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void SetRowVersion(
        CustodyTransfer transfer,
        byte[] rowVersion)
    {
        var property =
            typeof(CustodyTransfer).GetProperty(
                nameof(CustodyTransfer.RowVersion),
                BindingFlags.Instance |
                BindingFlags.Public);

        Assert.NotNull(property);

        property.SetValue(
            transfer,
            rowVersion);
    }
}