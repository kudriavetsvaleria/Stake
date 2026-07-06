namespace Stake.Application.Friendships.AcceptFriendRequest;

/// <summary>
/// Input for accepting a friend request: which request, and who is accepting it
/// (only the addressee may accept).
/// </summary>
public record AcceptFriendRequestCommand(Guid FriendshipId, Guid CurrentParticipantId);
