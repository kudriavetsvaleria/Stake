using Stake.Domain.Common;
using Stake.Domain.Entities;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class ChallengeSummaryTests
{
    private static readonly DateOnly CompletedAt = new(2026, 2, 1);

    private static ChallengeSummary CreateWith(Debt finalDebt) =>
        ChallengeSummary.Create(
            challengeId: Guid.NewGuid(),
            participantId: Guid.NewGuid(),
            completedAt: CompletedAt,
            totalWorked: TimeSpan.FromHours(50),
            daysWorked: 20,
            daysOffUsed: 2,
            finalDebt: finalDebt);

    [Fact]
    public void Create_WithValidData_CreatesSummary()
    {
        var summary = CreateWith(Debt.Zero);

        Assert.NotEqual(Guid.Empty, summary.Id);
        Assert.Equal(CompletedAt, summary.CompletedAt);
        Assert.Equal(TimeSpan.FromHours(50), summary.TotalWorked);
        Assert.Equal(20, summary.DaysWorked);
        Assert.Equal(2, summary.DaysOffUsed);
    }

    [Fact]
    public void IsWinner_WhenNoPrincipalAndNoInterest_IsTrue()
    {
        // Credit left over is fine, it does not affect winning.
        var debt = Debt.Create(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(3));

        var summary = CreateWith(debt);

        Assert.True(summary.IsWinner);
    }

    [Fact]
    public void IsWinner_WhenPrincipalRemains_IsFalse()
    {
        var debt = Debt.Create(TimeSpan.FromHours(1), TimeSpan.Zero, TimeSpan.Zero);

        var summary = CreateWith(debt);

        Assert.False(summary.IsWinner);
    }

    [Fact]
    public void IsWinner_WhenInterestRemains_IsFalse()
    {
        var debt = Debt.Create(TimeSpan.Zero, TimeSpan.FromHours(1), TimeSpan.Zero);

        var summary = CreateWith(debt);

        Assert.False(summary.IsWinner);
    }

    [Fact]
    public void Create_WithNegativeTotalWorked_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeSummary.Create(
            Guid.NewGuid(), Guid.NewGuid(), CompletedAt, TimeSpan.FromHours(-1), 20, 2, Debt.Zero));
    }

    [Fact]
    public void Create_WithNegativeDaysWorked_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeSummary.Create(
            Guid.NewGuid(), Guid.NewGuid(), CompletedAt, TimeSpan.FromHours(50), -1, 2, Debt.Zero));
    }

    [Fact]
    public void Create_WithNegativeDaysOffUsed_Throws()
    {
        Assert.Throws<DomainException>(() => ChallengeSummary.Create(
            Guid.NewGuid(), Guid.NewGuid(), CompletedAt, TimeSpan.FromHours(50), 20, -1, Debt.Zero));
    }
}
