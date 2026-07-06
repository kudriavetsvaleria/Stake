using Stake.Application.Common.Exceptions;
using Stake.Application.Friendships.SendFriendRequest;
using Stake.Application.Tests.Fakes;
using Stake.Domain.Common;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Friendships;

public class SendFriendRequestHandlerTests
{
    private static Participant NewParticipant(long telegramId, string nickname) =>
        Participant.Create(telegramId, nickname);

    [Fact]
    public async Task Handle_WithValidRequest_CreatesPendingFriendshipAndSaves()
    {
        var requester = NewParticipant(1, "vale");
        var addressee = NewParticipant(2, "bob");
        var participants = new FakeParticipantRepository(requester, addressee);
        var friendships = new FakeFriendshipRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SendFriendRequestHandler(participants, friendships, unitOfWork);

        var friendshipId = await handler.HandleAsync(
            new SendFriendRequestCommand(requester.Id, "bob"));

        var created = Assert.Single(friendships.Friendships);
        Assert.Equal(friendshipId, created.Id);
        Assert.Equal(requester.Id, created.RequesterId);
        Assert.Equal(addressee.Id, created.AddresseeId);
        Assert.Equal(FriendshipStatus.Pending, created.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenAddresseeNicknameNotFound_ThrowsNotFound()
    {
        var requester = NewParticipant(1, "vale");
        var participants = new FakeParticipantRepository(requester);
        var friendships = new FakeFriendshipRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SendFriendRequestHandler(participants, friendships, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new SendFriendRequestCommand(requester.Id, "ghost")));

        Assert.Empty(friendships.Friendships);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenFriendshipAlreadyExists_ThrowsConflict()
    {
        var requester = NewParticipant(1, "vale");
        var addressee = NewParticipant(2, "bob");
        var existing = Friendship.Create(requester.Id, addressee.Id);
        var participants = new FakeParticipantRepository(requester, addressee);
        var friendships = new FakeFriendshipRepository(existing);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SendFriendRequestHandler(participants, friendships, unitOfWork);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new SendFriendRequestCommand(requester.Id, "bob")));

        Assert.Single(friendships.Friendships); // still only the pre-existing one
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenRequesterEqualsAddressee_ThrowsDomainException()
    {
        var self = NewParticipant(1, "vale");
        var participants = new FakeParticipantRepository(self);
        var friendships = new FakeFriendshipRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new SendFriendRequestHandler(participants, friendships, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SendFriendRequestCommand(self.Id, "vale")));

        Assert.Empty(friendships.Friendships);
        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
