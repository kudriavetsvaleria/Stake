using Stake.Application.Common.Exceptions;
using Stake.Application.Participants.RegisterParticipant;
using Stake.Application.Tests.Fakes;
using Stake.Domain.Common;
using Stake.Domain.Entities;

namespace Stake.Application.Tests.Participants;

public class RegisterParticipantHandlerTests
{
    [Fact]
    public async Task Handle_WithNewUserAndFreeNickname_CreatesParticipantAndSaves()
    {
        var participants = new FakeParticipantRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterParticipantHandler(participants, unitOfWork);

        var id = await handler.HandleAsync(new RegisterParticipantCommand(12345, "vale"));

        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenTelegramUserAlreadyRegistered_ThrowsConflict()
    {
        var existing = Participant.Create(12345, "vale");
        var participants = new FakeParticipantRepository(existing);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterParticipantHandler(participants, unitOfWork);

        // Same Telegram id, different nickname -> still a conflict.
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RegisterParticipantCommand(12345, "other")));

        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WhenNicknameTaken_ThrowsConflict()
    {
        var existing = Participant.Create(12345, "vale");
        var participants = new FakeParticipantRepository(existing);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterParticipantHandler(participants, unitOfWork);

        // Different Telegram user, but the nickname is already taken.
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RegisterParticipantCommand(99999, "vale")));

        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_WithInvalidNickname_ThrowsDomainException()
    {
        var participants = new FakeParticipantRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterParticipantHandler(participants, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new RegisterParticipantCommand(12345, "   ")));

        Assert.Equal(0, unitOfWork.SaveCount);
    }
}
