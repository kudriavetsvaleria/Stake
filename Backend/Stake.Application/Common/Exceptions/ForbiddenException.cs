namespace Stake.Application.Common.Exceptions;

/// <summary>
/// The caller is authenticated but not allowed to perform this action on this
/// resource (e.g. accepting a request that was not sent to them). The API layer
/// maps this to HTTP 403.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
