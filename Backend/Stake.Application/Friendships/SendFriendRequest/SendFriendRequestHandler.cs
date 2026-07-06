using Stake.Application.Abstractions;
using Stake.Application.Abstractions.Persistence;
using Stake.Application.Common.Exceptions;
using Stake.Domain.Entities;

namespace Stake.Application.Friendships.SendFriendRequest;

public class SendFriendRequestHandler
{
    private readonly IParticipantRepository _participants;
    private readonly IFriendshipRepository _friendships;
    private readonly IUnitOfWork _unitOfWork;

    public SendFriendRequestHandler(
        IParticipantRepository participants,
        IFriendshipRepository friendships,
        IUnitOfWork unitOfWork)
    {
        _participants = participants;
        _friendships = friendships;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(SendFriendRequestCommand command, CancellationToken cancellationToken = default)
    {
        // Find the person being invited by their nickname.
        var addressee = await _participants.GetByNicknameAsync(command.AddresseeNickname, cancellationToken)
            ?? throw new NotFoundException($"No participant with nickname '{command.AddresseeNickname}'.");

        // Domain guards the single-object invariant (cannot befriend yourself).
        var friendship = Friendship.Create(command.RequesterId, addressee.Id);

        // Application guards the rule that needs the database: no duplicate request.
        if (await _friendships.ExistsBetweenAsync(command.RequesterId, addressee.Id, cancellationToken))
            throw new ConflictException("A friendship or pending request already exists between these participants.");

        await _friendships.AddAsync(friendship, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return friendship.Id;
    }
}
