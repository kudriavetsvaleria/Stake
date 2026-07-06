using Stake.Domain.Entities;

namespace Stake.Application.Abstractions.Persistence;

public interface IFriendshipRepository
{
    /// <summary>
    /// True if any friendship already links these two participants, in either
    /// direction. Declined requests are deleted, so an existing row means the
    /// pair is either pending or already friends.
    /// </summary>
    Task<bool> ExistsBetweenAsync(Guid participantA, Guid participantB, CancellationToken cancellationToken = default);

    Task AddAsync(Friendship friendship, CancellationToken cancellationToken = default);
}
