using Stake.Application.Abstractions;
using Stake.Application.Abstractions.Persistence;
using Stake.Application.Common.Exceptions;
using Stake.Domain.Entities;

namespace Stake.Application.Participants.RegisterParticipant;

public class RegisterParticipantHandler
{
    private readonly IParticipantRepository _participants;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterParticipantHandler(IParticipantRepository participants, IUnitOfWork unitOfWork)
    {
        _participants = participants;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(RegisterParticipantCommand command, CancellationToken cancellationToken = default)
    {
        // One Telegram account maps to exactly one participant.
        if (await _participants.ExistsByTelegramUserIdAsync(command.TelegramUserId, cancellationToken))
            throw new ConflictException("This Telegram user is already registered.");

        // Nicknames are unique across the system (people find each other by nickname).
        if (await _participants.ExistsByNicknameAsync(command.Nickname, cancellationToken))
            throw new ConflictException($"The nickname '{command.Nickname}' is already taken.");

        // Domain validates the shape of the data (positive id, non-empty nickname).
        var participant = Participant.Create(command.TelegramUserId, command.Nickname);

        await _participants.AddAsync(participant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return participant.Id;
    }
}
