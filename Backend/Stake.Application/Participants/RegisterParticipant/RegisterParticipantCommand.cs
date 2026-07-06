namespace Stake.Application.Participants.RegisterParticipant;

/// <summary>
/// Input for registering a new participant from Telegram: their Telegram user id
/// and the nickname they will be known by (defaults to their Telegram name, but
/// can be changed later).
/// </summary>
public record RegisterParticipantCommand(long TelegramUserId, string Nickname);
