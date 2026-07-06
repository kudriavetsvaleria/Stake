namespace Stake.Application.Friendships.SendFriendRequest;

/// <summary>
/// Input for sending a friend request: who is asking, and the nickname of the
/// person they want to befriend.
/// </summary>
public record SendFriendRequestCommand(Guid RequesterId, string AddresseeNickname);
