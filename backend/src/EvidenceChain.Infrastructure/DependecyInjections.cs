using EvidenceChain.Application.Common.Interfaces;
using EvidenceChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EvidenceChain.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        services.AddDbContext<EvidenceChainDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IEvidenceChainDbContext>(
            provider => provider.GetRequiredService<EvidenceChainDbContext>());

        return services;
    }
}