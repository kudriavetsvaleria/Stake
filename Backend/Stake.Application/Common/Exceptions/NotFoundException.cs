namespace Stake.Application.Common.Exceptions;

/// <summary>
/// A requested resource does not exist. The API layer maps this to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
