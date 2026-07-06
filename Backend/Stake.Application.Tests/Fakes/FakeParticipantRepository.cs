using Stake.Application.Abstractions.Persistence;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Fakes;

/// <summary>
/// In-memory participant store. Lets handler tests run the real logic without a database.
/// </summary>
public class FakeParticipantRepository : IParticipantRepository
{
    private readonly List<Participant> _participants = new();

    public FakeParticipantRepository(params Participant[] seed) => _participants.AddRange(seed);

    public Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_participants.FirstOrDefault(p => p.Id == id));

    public Task<Participant?> GetByNicknameAsync(string nickname, CancellationToken cancellationToken = default) =>
        Task.FromResult(_participants.FirstOrDefault(p => p.Nickname == nickname));

    public Task<bool> ExistsByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_participants.Any(p => p.TelegramUserId == telegramUserId));

    public Task<bool> ExistsByNicknameAsync(string nickname, CancellationToken cancellationToken = default) =>
        Task.FromResult(_participants.Any(p => p.Nickname == nickname));

    public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        _participants.Add(participant);
        return Task.CompletedTask;
    }
}
