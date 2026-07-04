using Stake.Domain.Common;
using Stake.Domain.Entities;

namespace Stake.Domain.Tests;

public class FriendshipTests
{
    [Fact]
    public void Create_WithDifferentParticipants_CreatesPendingFriendship()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();

        var friendship = Friendship.Create(requesterId, addresseeId);

        Assert.Equal(requesterId, friendship.RequesterId);
        Assert.Equal(addresseeId, friendship.AddresseeId);
        Assert.Equal(FriendshipStatus.Pending, friendship.Status);
        Assert.Null(friendship.RespondedAt);
    }

    [Fact]
    public void Create_WithSameRequesterAndAddressee_Throws()
    {
        var participantId = Guid.NewGuid();

        Assert.Throws<DomainException>(() => Friendship.Create(participantId, participantId));
    }

    [Fact]
    public void Accept_WhenPending_SetsAcceptedStatusAndRespondedAt()
    {
        var friendship = Friendship.Create(Guid.NewGuid(), Guid.NewGuid());

        friendship.Accept();

        Assert.Equal(FriendshipStatus.Accepted, friendship.Status);
        Assert.NotNull(friendship.RespondedAt);
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_Throws()
    {
        var friendship = Friendship.Create(Guid.NewGuid(), Guid.NewGuid());
        friendship.Accept();

        Assert.Throws<DomainException>(() => friendship.Accept());
    }
}
