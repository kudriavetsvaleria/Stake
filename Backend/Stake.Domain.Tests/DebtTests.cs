using Stake.Domain.Common;
using Stake.Domain.Entities;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class DebtTests
{
    [Fact]
    public void Create_WithValidValues_CreatesDebt()
    {
        var debt = Debt.Create(TimeSpan.FromHours(2), TimeSpan.FromHours(1), TimeSpan.FromMinutes(30));

        Assert.Equal(TimeSpan.FromHours(2), debt.Principal);
        Assert.Equal(TimeSpan.FromHours(1), debt.Interest);
        Assert.Equal(TimeSpan.FromMinutes(30), debt.Credit);
    }

    [Fact]
    public void Create_WithNegativePrincipal_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Debt.Create(TimeSpan.FromHours(-1), TimeSpan.Zero, TimeSpan.Zero));
    }

    [Fact]
    public void Create_WithNegativeInterest_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Debt.Create(TimeSpan.Zero, TimeSpan.FromHours(-1), TimeSpan.Zero));
    }

    [Fact]
    public void Create_WithNegativeCredit_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Debt.Create(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(-1)));
    }

    [Fact]
    public void ApplyWork_WhenWorkedLessThanNorm_ShortfallBecomesNewPrincipal()
    {
        // penaltyPercent: 0 isolates "today's norm joins the principal" from interest charging
        var debt = Debt.Zero.ApplyWork(
            worked: TimeSpan.FromHours(2), todayNorm: TimeSpan.FromHours(4), penaltyPercent: 0, PenaltyMode.Simple);

        Assert.Equal(TimeSpan.FromHours(2), debt.Principal);
        Assert.Equal(TimeSpan.Zero, debt.Interest);
        Assert.Equal(TimeSpan.Zero, debt.Credit);
    }

    [Fact]
    public void ApplyWork_WhenWorkedExceedsNorm_ExcessGoesToCredit()
    {
        var debt = Debt.Zero.ApplyWork(
            worked: TimeSpan.FromHours(6), todayNorm: TimeSpan.FromHours(4), penaltyPercent: 0.1, PenaltyMode.Simple);

        Assert.Equal(TimeSpan.Zero, debt.Principal);
        Assert.Equal(TimeSpan.Zero, debt.Interest);
        Assert.Equal(TimeSpan.FromHours(2), debt.Credit);
    }

    [Fact]
    public void ApplyWork_PaysOffInterestThenPrincipal_ThenChargesInterestOnWhatRemains()
    {
        var existingDebt = Debt.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(1), TimeSpan.Zero);

        var debt = existingDebt.ApplyWork(
            worked: TimeSpan.FromHours(5), todayNorm: TimeSpan.FromHours(4), penaltyPercent: 0.1, PenaltyMode.Simple);

        // today's norm joins principal first: 10h + 4h = 14h
        // worked 5h: pays off 1h interest, then 4h of the 14h principal -> principal 10h
        // fresh interest charged on what's left unpaid: 10h * 0.1 = 1h
        Assert.Equal(TimeSpan.FromHours(10), debt.Principal);
        Assert.Equal(TimeSpan.FromHours(1), debt.Interest);
        Assert.Equal(TimeSpan.Zero, debt.Credit);
    }

    [Fact]
    public void ApplyWork_TwoMissedDaysInARow_Simple_MatchesManualCalculation()
    {
        var norm = TimeSpan.FromHours(2);
        const double penaltyPercent = 0.5;

        var afterDay1 = Debt.Zero.ApplyWork(TimeSpan.Zero, norm, penaltyPercent, PenaltyMode.Simple);
        Assert.Equal(TimeSpan.FromHours(2), afterDay1.Principal);
        Assert.Equal(TimeSpan.FromHours(1), afterDay1.Interest);

        var afterDay2 = afterDay1.ApplyWork(TimeSpan.Zero, norm, penaltyPercent, PenaltyMode.Simple);
        Assert.Equal(TimeSpan.FromHours(4), afterDay2.Principal);
        Assert.Equal(TimeSpan.FromHours(3), afterDay2.Interest);

        var owedEnteringDay3 = afterDay2.Principal + afterDay2.Interest + norm;
        Assert.Equal(TimeSpan.FromHours(9), owedEnteringDay3);
    }

    [Fact]
    public void ApplyWork_TwoMissedDaysInARow_Compound_MatchesManualCalculation()
    {
        var norm = TimeSpan.FromHours(2);
        const double penaltyPercent = 0.5;

        var afterDay1 = Debt.Zero.ApplyWork(TimeSpan.Zero, norm, penaltyPercent, PenaltyMode.Compound);
        Assert.Equal(TimeSpan.FromHours(2), afterDay1.Principal);
        Assert.Equal(TimeSpan.FromHours(1), afterDay1.Interest);

        var afterDay2 = afterDay1.ApplyWork(TimeSpan.Zero, norm, penaltyPercent, PenaltyMode.Compound);
        Assert.Equal(TimeSpan.FromHours(4), afterDay2.Principal);
        Assert.Equal(TimeSpan.FromHours(3.5), afterDay2.Interest);

        var owedEnteringDay3 = afterDay2.Principal + afterDay2.Interest + norm;
        Assert.Equal(TimeSpan.FromHours(9.5), owedEnteringDay3);
    }

    [Fact]
    public void ApplyWork_SimpleMode_ChargesInterestOnPrincipalOnly()
    {
        var existingDebt = Debt.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(5), TimeSpan.Zero);

        var debt = existingDebt.ApplyWork(
            worked: TimeSpan.Zero, todayNorm: TimeSpan.Zero, penaltyPercent: 0.1, PenaltyMode.Simple);

        // 10h * 0.1 = 1h charged, old interest (5h) is not itself taxed
        Assert.Equal(TimeSpan.FromHours(6), debt.Interest);
        Assert.Equal(TimeSpan.FromHours(10), debt.Principal);
    }

    [Fact]
    public void ApplyWork_CompoundMode_ChargesInterestOnPrincipalAndInterest()
    {
        var existingDebt = Debt.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(5), TimeSpan.Zero);

        var debt = existingDebt.ApplyWork(
            worked: TimeSpan.Zero, todayNorm: TimeSpan.Zero, penaltyPercent: 0.1, PenaltyMode.Compound);

        // (10h + 5h) * 0.1 = 1.5h charged
        Assert.Equal(TimeSpan.FromHours(6.5), debt.Interest);
        Assert.Equal(TimeSpan.FromHours(10), debt.Principal);
    }

    [Fact]
    public void ApplyWork_OnWeeklyMinimumDay_ExtraOverMinimumGoesToCredit()
    {
        var rules = ChallengeRules.Create(
            hoursNorm: TimeSpan.FromHours(2), penaltyPercent: 0.1, maxDaysOff: 3,
            weeklyMinimum: TimeSpan.FromHours(1), PenaltyMode.Simple);
        var norm = rules.GetNormForDay(DayType.WeeklyMinimum);

        // worked 1.5h against a 1h weekly minimum -> 0.5h excess
        var debt = Debt.Zero.ApplyWork(TimeSpan.FromMinutes(90), norm, rules.PenaltyPercent, rules.PenaltyMode);

        Assert.Equal(TimeSpan.Zero, debt.Principal);
        Assert.Equal(TimeSpan.Zero, debt.Interest);
        Assert.Equal(TimeSpan.FromMinutes(30), debt.Credit);
    }

    [Fact]
    public void ApplyWork_OnDayOff_AllWorkedTimeGoesToCredit()
    {
        var rules = ChallengeRules.Create(
            hoursNorm: TimeSpan.FromHours(2), penaltyPercent: 0.1, maxDaysOff: 3,
            weeklyMinimum: TimeSpan.FromHours(1), PenaltyMode.Simple);
        var norm = rules.GetNormForDay(DayType.DayOff);

        // worked 1.5h on a day off (norm 0) -> all 1.5h is credit
        var debt = Debt.Zero.ApplyWork(TimeSpan.FromMinutes(90), norm, rules.PenaltyPercent, rules.PenaltyMode);

        Assert.Equal(TimeSpan.Zero, debt.Principal);
        Assert.Equal(TimeSpan.Zero, debt.Interest);
        Assert.Equal(TimeSpan.FromMinutes(90), debt.Credit);
    }

    [Fact]
    public void UseCredit_WhenAmountExceedsAvailableCredit_Throws()
    {
        var debt = Debt.Create(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(1));

        Assert.Throws<DomainException>(() => debt.UseCredit(TimeSpan.FromHours(2)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UseCredit_WithNonPositiveAmount_Throws(int hours)
    {
        var debt = Debt.Create(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(5));

        Assert.Throws<DomainException>(() => debt.UseCredit(TimeSpan.FromHours(hours)));
    }

    [Fact]
    public void UseCredit_PaysInterestBeforePrincipal()
    {
        var debt = Debt.Create(TimeSpan.FromHours(5), TimeSpan.FromHours(2), TimeSpan.FromHours(10));

        var result = debt.UseCredit(TimeSpan.FromHours(4));

        // 4h spent: 2h clears Interest, remaining 2h reduces Principal from 5h to 3h
        Assert.Equal(TimeSpan.FromHours(3), result.Principal);
        Assert.Equal(TimeSpan.Zero, result.Interest);
        Assert.Equal(TimeSpan.FromHours(6), result.Credit);
    }

    [Fact]
    public void UseCredit_WhenAmountExceedsTotalDebt_OnlySpendsWhatIsOwed()
    {
        var debt = Debt.Create(TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.FromHours(10));

        var result = debt.UseCredit(TimeSpan.FromHours(5));

        Assert.Equal(TimeSpan.Zero, result.Principal);
        Assert.Equal(TimeSpan.Zero, result.Interest);
        Assert.Equal(TimeSpan.FromHours(9), result.Credit);
    }
}
