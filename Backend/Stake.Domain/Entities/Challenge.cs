using Stake.Domain.Common;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Entities;

public class Challenge
{
    public Guid Id { get; private set; }
    public int DurationInDays { get; private set; }
    public DateTime StartDate { get; private set; }
    public ChallengeRules Rules { get; private set; }

    private Challenge(Guid id, int durationInDays, DateTime startDate, ChallengeRules rules)
    {
        Id = id;
        DurationInDays = durationInDays;
        StartDate = startDate;
        Rules = rules;
    }

    public static Challenge Create(int durationInDays, DateTime startDate, ChallengeRules rules)
    {
        if (durationInDays <= 0)
            throw new DomainException("DurationInDays must be positive.");

        return new Challenge(Guid.NewGuid(), durationInDays, startDate, rules);
    }

    public void UpdateRules(
        TimeSpan? hoursNorm = null,
        double? penaltyPercent = null,
        int? maxDaysOff = null,
        PenaltyMode? penaltyMode = null,
        bool updateWeeklyMinimum = false,
        TimeSpan? weeklyMinimum = null)
    {
        Rules = Rules.Update(hoursNorm, penaltyPercent, maxDaysOff, penaltyMode, updateWeeklyMinimum, weeklyMinimum);
    }
}
