using Stake.Application.Abstractions;
using Stake.Application.Abstractions.Persistence;
using Stake.Application.Common.Exceptions;

namespace Stake.Application.Friendships.DeclineFriendRequest;

public class DeclineFriendRequestHandler
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public DeclineFriendRequestHandler(IFriendshipRepository friendships, IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeclineFriendRequestCommand command, CancellationToken cancellationToken = default)
    {
        var friendship = await _friendships.GetByIdAsync(command.FriendshipId, cancellationToken)
            ?? throw new NotFoundException($"No friend request with id '{command.FriendshipId}'.");

        // Only the person the request was sent to may decline it.
        if (friendship.AddresseeId != command.CurrentParticipantId)
            throw new ForbiddenException("Only the addressee can decline this friend request.");

        // Declining deletes the record: a later request becomes a fresh row.
        _friendships.Remove(friendship);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
