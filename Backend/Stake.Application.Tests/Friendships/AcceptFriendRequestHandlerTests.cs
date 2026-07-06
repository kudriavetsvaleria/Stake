using Stake.Application.Common.Exceptions;
using Stake.Application.Friendships.AcceptFriendRequest;
using Stake.Application.Tests.Fakes;
using Stake.Domain.Common;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Friendships;

public class AcceptFriendRequestHandlerTests
{
    [Fact]
    public async Task Handle_WhenAddresseeAccepts_SetsAcceptedAndSaves()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = Friendship.Create(requesterId, addresseeId);
        var friendships = new FakeFriendshipRepository(friendship);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AcceptFriendRequestHandler(friendships, unitOfWork);

        await handler.HandleAsync(new AcceptFriendRequestCommand(friendship.Id, addresseeId));

        Assert.Equal(FriendshipStatus.Accepted, friendship.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenFriendshipNotFound_ThrowsNotFound()
    {
        var friendships = new FakeFriendshipRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AcceptFriendRequestHandler(friendships, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new AcceptFriendRequestCommand(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenNotTheAddressee_ThrowsForbidden()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = Friendship.Create(requesterId, addresseeId);
        var friendships = new FakeFriendshipRepository(friendship);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AcceptFriendRequestHandler(friendships, unitOfWork);

        // The requester tries to accept their own request -> forbidden.
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.HandleAsync(new AcceptFriendRequestCommand(friendship.Id, requesterId)));

        Assert.Equal(FriendshipStatus.Pending, friendship.Status);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenAlreadyAccepted_ThrowsDomainException()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = Friendship.Create(requesterId, addresseeId);
        friendship.Accept();
        var friendships = new FakeFriendshipRepository(friendship);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new AcceptFriendRequestHandler(friendships, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new AcceptFriendRequestCommand(friendship.Id, addresseeId)));

        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
