using Stake.Domain.Common;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Entities;

public class ChallengeSummary
{
    public Guid Id { get; private set; }
    public Guid ChallengeId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public DateOnly CompletedAt { get; private set; }
    public TimeSpan TotalWorked { get; private set; }
    public int DaysWorked { get; private set; }
    public int DaysOffUsed { get; private set; }
    public Debt FinalDebt { get; private set; }

    // Derived from FinalDebt so it can never disagree with the actual debt.
    public bool IsWinner => FinalDebt.Principal == TimeSpan.Zero && FinalDebt.Interest == TimeSpan.Zero;

    private ChallengeSummary(
        Guid id,
        Guid challengeId,
        Guid participantId,
        DateOnly completedAt,
        TimeSpan totalWorked,
        int daysWorked,
        int daysOffUsed,
        Debt finalDebt)
    {
        Id = id;
        ChallengeId = challengeId;
        ParticipantId = participantId;
        CompletedAt = completedAt;
        TotalWorked = totalWorked;
        DaysWorked = daysWorked;
        DaysOffUsed = daysOffUsed;
        FinalDebt = finalDebt;
    }

    public static ChallengeSummary Create(
        Guid challengeId,
        Guid participantId,
        DateOnly completedAt,
        TimeSpan totalWorked,
        int daysWorked,
        int daysOffUsed,
        Debt finalDebt)
    {
        if (totalWorked < TimeSpan.Zero)
            throw new DomainException("TotalWorked cannot be negative.");

        if (daysWorked < 0)
            throw new DomainException("DaysWorked cannot be negative.");

        if (daysOffUsed < 0)
            throw new DomainException("DaysOffUsed cannot be negative.");

        return new ChallengeSummary(
            Guid.NewGuid(),
            challengeId,
            participantId,
            completedAt,
            totalWorked,
            daysWorked,
            daysOffUsed,
            finalDebt);
    }
}
