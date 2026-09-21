namespace EvidenceChain.Application.Common.Exceptions;

public sealed class ConcurrencyConflictException
    : Exception
{
    public ConcurrencyConflictException(
        string message,
        object currentState)
        : base(message)
    {
        CurrentState = currentState;
    }

    public object CurrentState { get; }
}