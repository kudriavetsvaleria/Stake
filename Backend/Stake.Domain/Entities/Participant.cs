using Stake.Domain.Common;

namespace Stake.Domain.Entities;

public class Participant
{
    public Guid Id { get; private set; }
    public long TelegramUserId { get; private set; }
    public string Nickname { get; private set; }

    private Participant(Guid id, long telegramUserId, string nickname)
    {
        Id = id;
        TelegramUserId = telegramUserId;
        Nickname = nickname;
    }

    public static Participant Create(long telegramUserId, string nickname)
    {
        if (telegramUserId <= 0)
            throw new DomainException("TelegramUserId must be positive.");

        ValidateNickname(nickname);

        return new Participant(Guid.NewGuid(), telegramUserId, nickname);
    }

    public void Rename(string newNickname)
    {
        ValidateNickname(newNickname);
        Nickname = newNickname;
    }

    private static void ValidateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new DomainException("Nickname cannot be empty.");
    }
}
