using EvidenceChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EvidenceChain.IntegrationTests.TestInfrastructure;

internal static class TestDbContextFactory
{
    public static EvidenceChainDbContext Create()
    {
        var databaseName =
            $"EvidenceChainTests-{Guid.NewGuid()}";

        var options =
            new DbContextOptionsBuilder<
                    EvidenceChainDbContext>()
                .UseInMemoryDatabase(databaseName)
                .EnableSensitiveDataLogging()
                .Options;

        var dbContext =
            new EvidenceChainDbContext(options);

        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}