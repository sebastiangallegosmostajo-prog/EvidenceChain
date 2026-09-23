namespace EvidenceChain.IntegrationTests.TestInfrastructure;

internal sealed class FixedTimeProvider
    : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(
        DateTimeOffset utcNow)
    {
        _utcNow =
            utcNow.ToUniversalTime();
    }

    public override DateTimeOffset GetUtcNow()
    {
        return _utcNow;
    }
}