using Stake.Application.Abstractions;
using Stake.Application.Abstractions.Persistence;
using Stake.Application.Common.Exceptions;

namespace Stake.Application.Friendships.AcceptFriendRequest;

public class AcceptFriendRequestHandler
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptFriendRequestHandler(IFriendshipRepository friendships, IUnitOfWork unitOfWork)
    {
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AcceptFriendRequestCommand command, CancellationToken cancellationToken = default)
    {
        var friendship = await _friendships.GetByIdAsync(command.FriendshipId, cancellationToken)
            ?? throw new NotFoundException($"No friend request with id '{command.FriendshipId}'.");

        // Only the person the request was sent to may accept it.
        if (friendship.AddresseeId != command.CurrentParticipantId)
            throw new ForbiddenException("Only the addressee can accept this friend request.");

        // Domain guards the state transition (must still be pending).
        friendship.Accept();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
