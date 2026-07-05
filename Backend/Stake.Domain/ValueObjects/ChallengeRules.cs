using Stake.Domain.Common;

namespace Stake.Domain.ValueObjects;

public record ChallengeRules
{
    public TimeSpan HoursNorm { get; }
    public double PenaltyPercent { get; }
    public int MaxDaysOff { get; }
    public TimeSpan? WeeklyMinimum { get; }
    public PenaltyMode PenaltyMode { get; }

    private ChallengeRules(
        TimeSpan hoursNorm,
        double penaltyPercent,
        int maxDaysOff,
        TimeSpan? weeklyMinimum,
        PenaltyMode penaltyMode)
    {
        HoursNorm = hoursNorm;
        PenaltyPercent = penaltyPercent;
        MaxDaysOff = maxDaysOff;
        WeeklyMinimum = weeklyMinimum;
        PenaltyMode = penaltyMode;
    }

    public static ChallengeRules Create(
        TimeSpan hoursNorm,
        double penaltyPercent,
        int maxDaysOff,
        TimeSpan? weeklyMinimum,
        PenaltyMode penaltyMode)
    {
        if (hoursNorm <= TimeSpan.Zero || hoursNorm > TimeSpan.FromHours(24))
            throw new DomainException("HoursNorm must be greater than 0 and at most 24 hours.");

        if (penaltyPercent < 0)
            throw new DomainException("PenaltyPercent cannot be negative.");

        if (maxDaysOff < 0)
            throw new DomainException("MaxDaysOff cannot be negative.");

        if (weeklyMinimum is not null)
        {
            if (weeklyMinimum.Value < TimeSpan.Zero)
                throw new DomainException("WeeklyMinimum cannot be negative.");

            if (weeklyMinimum.Value >= hoursNorm)
                throw new DomainException("WeeklyMinimum must be less than HoursNorm.");
        }

        return new ChallengeRules(hoursNorm, penaltyPercent, maxDaysOff, weeklyMinimum, penaltyMode);
    }

    public ChallengeRules Update(
        TimeSpan? hoursNorm = null,
        double? penaltyPercent = null,
        int? maxDaysOff = null,
        PenaltyMode? penaltyMode = null,
        bool updateWeeklyMinimum = false,
        TimeSpan? weeklyMinimum = null)
    {
        return Create(
            hoursNorm ?? HoursNorm,
            penaltyPercent ?? PenaltyPercent,
            maxDaysOff ?? MaxDaysOff,
            updateWeeklyMinimum ? weeklyMinimum : WeeklyMinimum,
            penaltyMode ?? PenaltyMode);
    }
}

public enum PenaltyMode
{
    Simple,
    Compound
}
