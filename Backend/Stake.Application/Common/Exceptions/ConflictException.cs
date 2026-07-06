namespace Stake.Application.Common.Exceptions;

/// <summary>
/// The request clashes with the current state (e.g. a duplicate). The API layer
/// maps this to HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
