using Stake.Application.Abstractions.Persistence;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Fakes;

/// <summary>
/// In-memory friendship store. Added friendships are visible so tests can assert on them.
/// </summary>
public class FakeFriendshipRepository : IFriendshipRepository
{
    public List<Friendship> Friendships { get; } = new();

    public FakeFriendshipRepository(params Friendship[] seed) => Friendships.AddRange(seed);

    public Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Friendships.FirstOrDefault(f => f.Id == id));

    public Task<bool> ExistsBetweenAsync(Guid participantA, Guid participantB, CancellationToken cancellationToken = default)
    {
        var exists = Friendships.Any(f =>
            (f.RequesterId == participantA && f.AddresseeId == participantB) ||
            (f.RequesterId == participantB && f.AddresseeId == participantA));

        return Task.FromResult(exists);
    }

    public Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default)
    {
        Friendships.Add(friendship);
        return Task.CompletedTask;
    }

    public void Remove(Friendship friendship) => Friendships.Remove(friendship);
}
