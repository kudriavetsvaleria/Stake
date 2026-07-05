using Stake.Domain.Common;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class ChallengeRulesTests
{
    private static ChallengeRules CreateValidRules() =>
        ChallengeRules.Create(
            hoursNorm: TimeSpan.FromHours(4),
            penaltyPercent: 0.1,
            maxDaysOff: 3,
            weeklyMinimum: TimeSpan.FromHours(1),
            penaltyMode: PenaltyMode.Simple);

    [Fact]
    public void Create_WithValidValues_CreatesRules()
    {
        var rules = CreateValidRules();

        Assert.Equal(TimeSpan.FromHours(4), rules.HoursNorm);
        Assert.Equal(0.1, rules.PenaltyPercent);
        Assert.Equal(3, rules.MaxDaysOff);
        Assert.Equal(TimeSpan.FromHours(1), rules.WeeklyMinimum);
        Assert.Equal(PenaltyMode.Simple, rules.PenaltyMode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(25)]
    public void Create_WithInvalidHoursNorm_Throws(int hours)
    {
        Assert.Throws<DomainException>(() => ChallengeRules.Create(
            TimeSpan.FromHours(hours), 0.1, 3, TimeSpan.FromHours(1), PenaltyMode.Simple));
    }

    [Fact]
    public void Create_WithNegativePenaltyPercent_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeRules.Create(
            TimeSpan.FromHours(4), -0.1, 3, TimeSpan.FromHours(1), PenaltyMode.Simple));
    }

    [Fact]
    public void Create_WithNegativeMaxDaysOff_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeRules.Create(
            TimeSpan.FromHours(4), 0.1, -1, TimeSpan.FromHours(1), PenaltyMode.Simple));
    }

    [Fact]
    public void Create_WithNegativeWeeklyMinimum_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeRules.Create(
            TimeSpan.FromHours(4), 0.1, 3, TimeSpan.FromHours(-1), PenaltyMode.Simple));
    }

    [Fact]
    public void Create_WithWeeklyMinimumNotLessThanHoursNorm_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeRules.Create(
            TimeSpan.FromHours(4), 0.1, 3, TimeSpan.FromHours(4), PenaltyMode.Simple));
    }

    [Fact]
    public void TwoRulesWithSameValues_AreEqual()
    {
        var rules1 = CreateValidRules();
        var rules2 = CreateValidRules();

        Assert.Equal(rules1, rules2);
    }

    [Fact]
    public void Create_WithNullWeeklyMinimum_Succeeds()
    {
        var rules = ChallengeRules.Create(
            TimeSpan.FromHours(4), 0.1, 3, weeklyMinimum: null, PenaltyMode.Simple);

        Assert.Null(rules.WeeklyMinimum);
    }

    [Fact]
    public void Update_WithNoParameters_KeepsAllValues()
    {
        var rules = CreateValidRules();

        var updated = rules.Update();

        Assert.Equal(rules, updated);
    }

    [Fact]
    public void Update_ChangingOnlyHoursNorm_KeepsOtherValues()
    {
        var rules = CreateValidRules();

        var updated = rules.Update(hoursNorm: TimeSpan.FromHours(6));

        Assert.Equal(TimeSpan.FromHours(6), updated.HoursNorm);
        Assert.Equal(rules.PenaltyPercent, updated.PenaltyPercent);
        Assert.Equal(rules.MaxDaysOff, updated.MaxDaysOff);
        Assert.Equal(rules.WeeklyMinimum, updated.WeeklyMinimum);
        Assert.Equal(rules.PenaltyMode, updated.PenaltyMode);
    }

    [Fact]
    public void Update_WithoutTouchingWeeklyMinimum_KeepsExistingValue()
    {
        var rules = CreateValidRules();

        var updated = rules.Update(weeklyMinimum: TimeSpan.FromHours(3));

        Assert.Equal(rules.WeeklyMinimum, updated.WeeklyMinimum);
    }

    [Fact]
    public void Update_EnablingWeeklyMinimum_SetsNewValue()
    {
        var rules = ChallengeRules.Create(
            TimeSpan.FromHours(4), 0.1, 3, weeklyMinimum: null, PenaltyMode.Simple);

        var updated = rules.Update(updateWeeklyMinimum: true, weeklyMinimum: TimeSpan.FromHours(2));

        Assert.Equal(TimeSpan.FromHours(2), updated.WeeklyMinimum);
    }

    [Fact]
    public void Update_DisablingWeeklyMinimum_ClearsValue()
    {
        var rules = CreateValidRules();

        var updated = rules.Update(updateWeeklyMinimum: true, weeklyMinimum: null);

        Assert.Null(updated.WeeklyMinimum);
    }
}
