namespace Stake.Application.Friendships.DeclineFriendRequest;

/// <summary>
/// Input for declining a friend request: which request, and who is declining it
/// (only the addressee may decline).
/// </summary>
public record DeclineFriendRequestCommand(Guid FriendshipId, Guid CurrentParticipantId);
