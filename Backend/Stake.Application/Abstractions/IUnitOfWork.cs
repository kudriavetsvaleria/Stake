namespace Stake.Application.Abstractions;

/// <summary>
/// Commits all changes made through the repositories as a single transaction.
/// A use case calls this once, at the end, to persist its work.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
