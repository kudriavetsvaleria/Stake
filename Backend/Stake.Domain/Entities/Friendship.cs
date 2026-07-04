using Stake.Domain.Common;

namespace Stake.Domain.Entities;

public class Friendship
{
    public Guid Id { get; private set; }
    public Guid RequesterId { get; private set; }
    public Guid AddresseeId { get; private set; }
    public FriendshipStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private Friendship(
        Guid id,
        Guid requesterId,
        Guid addresseeId,
        FriendshipStatus status,
        DateTime createdAt,
        DateTime? respondedAt)
    {
        Id = id;
        RequesterId = requesterId;
        AddresseeId = addresseeId;
        Status = status;
        CreatedAt = createdAt;
        RespondedAt = respondedAt;
    }

    public static Friendship Create(Guid requesterId, Guid addresseeId)
    {
        if (requesterId == addresseeId)
            throw new DomainException("Cannot send a friend request to yourself.");

        return new Friendship(
            Guid.NewGuid(),
            requesterId,
            addresseeId,
            FriendshipStatus.Pending,
            DateTime.UtcNow,
            respondedAt: null);
    }

    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
            throw new DomainException("Only a pending friend request can be accepted.");

        Status = FriendshipStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }
}

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Declined
}
