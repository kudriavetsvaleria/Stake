using Stake.Domain.Entities;

namespace Stake.Application.Abstractions.Persistence;

public interface IParticipantRepository
{
    Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Participant?> GetByNicknameAsync(string nickname, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNicknameAsync(string nickname, CancellationToken cancellationToken = default);

    Task AddAsync(Participant participant, CancellationToken cancellationToken = default);
}
