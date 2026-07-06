using Stake.Application.Common.Exceptions;
using Stake.Application.Friendships.DeclineFriendRequest;
using Stake.Application.Tests.Fakes;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Friendships;

public class DeclineFriendRequestHandlerTests
{
    [Fact]
    public async Task Handle_WhenAddresseeDeclines_RemovesFriendshipAndSaves()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = Friendship.Create(requesterId, addresseeId);
        var friendships = new FakeFriendshipRepository(friendship);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new DeclineFriendRequestHandler(friendships, unitOfWork);

        await handler.HandleAsync(new DeclineFriendRequestCommand(friendship.Id, addresseeId));

        Assert.Empty(friendships.Friendships);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenFriendshipNotFound_ThrowsNotFound()
    {
        var friendships = new FakeFriendshipRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new DeclineFriendRequestHandler(friendships, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new DeclineFriendRequestCommand(Guid.NewGuid(), Guid.NewGuid())));

        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenNotTheAddressee_ThrowsForbiddenAndKeepsFriendship()
    {
        var requesterId = Guid.NewGuid();
        var addresseeId = Guid.NewGuid();
        var friendship = Friendship.Create(requesterId, addresseeId);
        var friendships = new FakeFriendshipRepository(friendship);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new DeclineFriendRequestHandler(friendships, unitOfWork);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.HandleAsync(new DeclineFriendRequestCommand(friendship.Id, requesterId)));

        Assert.Single(friendships.Friendships);
        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
