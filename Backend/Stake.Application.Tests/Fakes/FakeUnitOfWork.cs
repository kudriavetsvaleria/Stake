using Stake.Application.Abstractions;

namespace Stake.Application.Tests.Fakes;

/// <summary>
/// Records whether the use case committed, so tests can assert that saving happened.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}
