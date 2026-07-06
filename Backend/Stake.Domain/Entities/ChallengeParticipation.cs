using Stake.Domain.Common;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Entities;

public class ChallengeParticipation
{
    public Guid Id { get; private set; }
    public Guid ChallengeId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public bool IsAdmin { get; private set; }
    public DateTime JoinedAt { get; private set; }

    // Cached fields. Source of truth is the DayRecord history; these are updated
    // only by RecalculateFrom so that reads stay O(1) instead of replaying every day.
    public Debt Debt { get; private set; }
    public int RemainingDaysOff { get; private set; }
    public bool WeeklyMinimumUsedThisWeek { get; private set; }

    private ChallengeParticipation(
        Guid id,
        Guid challengeId,
        Guid participantId,
        bool isAdmin,
        DateTime joinedAt,
        Debt debt,
        int remainingDaysOff,
        bool weeklyMinimumUsedThisWeek)
    {
        Id = id;
        ChallengeId = challengeId;
        ParticipantId = participantId;
        IsAdmin = isAdmin;
        JoinedAt = joinedAt;
        Debt = debt;
        RemainingDaysOff = remainingDaysOff;
        WeeklyMinimumUsedThisWeek = weeklyMinimumUsedThisWeek;
    }

    public static ChallengeParticipation Create(
        Guid challengeId,
        Guid participantId,
        bool isAdmin,
        int maxDaysOff)
    {
        if (maxDaysOff < 0)
            throw new DomainException("MaxDaysOff cannot be negative.");

        return new ChallengeParticipation(
            Guid.NewGuid(),
            challengeId,
            participantId,
            isAdmin,
            DateTime.UtcNow,
            Debt.Zero,
            remainingDaysOff: maxDaysOff,
            weeklyMinimumUsedThisWeek: false);
    }

    public void RecalculateFrom(DayRecord dayRecord)
    {
        if (dayRecord.ChallengeParticipationId != Id)
            throw new DomainException("DayRecord belongs to a different participation.");

        if (!dayRecord.IsClosed || dayRecord.DebtSnapshot is null)
            throw new DomainException("Cannot recalculate from a day that is not closed.");

        // A new week starts on Monday: clear the weekly-minimum flag before applying today.
        if (dayRecord.Date.DayOfWeek == DayOfWeek.Monday)
            WeeklyMinimumUsedThisWeek = false;

        Debt = dayRecord.DebtSnapshot;

        switch (dayRecord.Type)
        {
            case DayType.DayOff:
                RemainingDaysOff = Math.Max(0, RemainingDaysOff - 1);
                break;
            case DayType.WeeklyMinimum:
                WeeklyMinimumUsedThisWeek = true;
                break;
        }
    }
}
