using Stake.Domain.Common;
using Stake.Domain.Entities;

namespace Stake.Domain.Tests;

public class ParticipantTests
{
    [Fact]
    public void Create_WithValidData_CreatesParticipant()
    {
        var participant = Participant.Create(telegramUserId: 12345, nickname: "vale");

        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal(12345, participant.TelegramUserId);
        Assert.Equal("vale", participant.Nickname);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveTelegramUserId_Throws(long telegramUserId)
    {
        Assert.Throws<DomainException>(() => Participant.Create(telegramUserId, "vale"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidNickname_Throws(string? nickname)
    {
        Assert.Throws<DomainException>(() => Participant.Create(12345, nickname!));
    }

    [Fact]
    public void Rename_WithValidNickname_UpdatesNickname()
    {
        var participant = Participant.Create(12345, "vale");

        participant.Rename("newVale");

        Assert.Equal("newVale", participant.Nickname);
    }

    [Fact]
    public void Rename_WithInvalidNickname_Throws()
    {
        var participant = Participant.Create(12345, "vale");

        Assert.Throws<DomainException>(() => participant.Rename(""));
    }
}
