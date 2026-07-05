using Stake.Domain.Common;
using Stake.Domain.Entities;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class ChallengeTests
{
    private static ChallengeRules CreateRules() =>
        ChallengeRules.Create(
            hoursNorm: TimeSpan.FromHours(4),
            penaltyPercent: 0.1,
            maxDaysOff: 3,
            weeklyMinimum: TimeSpan.FromHours(1),
            penaltyMode: PenaltyMode.Simple);

    [Fact]
    public void Create_WithValidData_CreatesChallenge()
    {
        var startDate = new DateTime(2026, 1, 1);
        var rules = CreateRules();

        var challenge = Challenge.Create(durationInDays: 30, startDate, rules);

        Assert.NotEqual(Guid.Empty, challenge.Id);
        Assert.Equal(30, challenge.DurationInDays);
        Assert.Equal(startDate, challenge.StartDate);
        Assert.Equal(rules, challenge.Rules);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithNonPositiveDuration_Throws(int durationInDays)
    {
        Assert.Throws<DomainException>(() => Challenge.Create(durationInDays, DateTime.UtcNow, CreateRules()));
    }

    [Fact]
    public void UpdateRules_ChangingOnlyOneField_KeepsOthersUnchanged()
    {
        var challenge = Challenge.Create(30, DateTime.UtcNow, CreateRules());

        challenge.UpdateRules(penaltyMode: PenaltyMode.Compound);

        Assert.Equal(PenaltyMode.Compound, challenge.Rules.PenaltyMode);
        Assert.Equal(TimeSpan.FromHours(4), challenge.Rules.HoursNorm);
        Assert.Equal(0.1, challenge.Rules.PenaltyPercent);
        Assert.Equal(3, challenge.Rules.MaxDaysOff);
        Assert.Equal(TimeSpan.FromHours(1), challenge.Rules.WeeklyMinimum);
    }

    [Fact]
    public void UpdateRules_DisablingWeeklyMinimum_ClearsIt()
    {
        var challenge = Challenge.Create(30, DateTime.UtcNow, CreateRules());

        challenge.UpdateRules(updateWeeklyMinimum: true, weeklyMinimum: null);

        Assert.Null(challenge.Rules.WeeklyMinimum);
    }
}
