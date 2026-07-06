using Stake.Domain.Common;
using Stake.Domain.Entities;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class ChallengeParticipationTests
{
    // A Monday and a non-Monday, used to exercise the weekly reset.
    private static readonly DateOnly Monday = new(2026, 1, 5);
    private static readonly DateOnly Tuesday = new(2026, 1, 6);

    private static ChallengeParticipation CreateParticipation(int maxDaysOff = 3) =>
        ChallengeParticipation.Create(
            challengeId: Guid.NewGuid(),
            participantId: Guid.NewGuid(),
            isAdmin: false,
            maxDaysOff: maxDaysOff);

    private static DayRecord ClosedDay(Guid participationId, DateOnly date, DayType type, Debt snapshot)
    {
        var day = DayRecord.Create(participationId, date, type);
        day.Close(snapshot);
        return day;
    }

    [Fact]
    public void Create_StartsWithZeroDebtAndFullDaysOff()
    {
        var participation = ChallengeParticipation.Create(Guid.NewGuid(), Guid.NewGuid(), isAdmin: true, maxDaysOff: 5);

        Assert.NotEqual(Guid.Empty, participation.Id);
        Assert.True(participation.IsAdmin);
        Assert.Equal(Debt.Zero, participation.Debt);
        Assert.Equal(5, participation.RemainingDaysOff);
        Assert.False(participation.WeeklyMinimumUsedThisWeek);
    }

    [Fact]
    public void Create_WithNegativeMaxDaysOff_Throws()
    {
        Assert.Throws<DomainException>(() =>
            ChallengeParticipation.Create(Guid.NewGuid(), Guid.NewGuid(), false, maxDaysOff: -1));
    }

    [Fact]
    public void RecalculateFrom_CopiesDebtSnapshotIntoCache()
    {
        var participation = CreateParticipation();
        var snapshot = Debt.Create(TimeSpan.FromHours(2), TimeSpan.FromHours(1), TimeSpan.Zero);
        var day = ClosedDay(participation.Id, Tuesday, DayType.Regular, snapshot);

        participation.RecalculateFrom(day);

        Assert.Equal(snapshot, participation.Debt);
    }

    [Fact]
    public void RecalculateFrom_DayOff_DecrementsRemainingDaysOff()
    {
        var participation = CreateParticipation(maxDaysOff: 2);
        var day = ClosedDay(participation.Id, Tuesday, DayType.DayOff, Debt.Zero);

        participation.RecalculateFrom(day);

        Assert.Equal(1, participation.RemainingDaysOff);
    }

    [Fact]
    public void RecalculateFrom_DayOff_NeverGoesBelowZero()
    {
        var participation = CreateParticipation(maxDaysOff: 0);
        var day = ClosedDay(participation.Id, Tuesday, DayType.DayOff, Debt.Zero);

        participation.RecalculateFrom(day);

        Assert.Equal(0, participation.RemainingDaysOff);
    }

    [Fact]
    public void RecalculateFrom_WeeklyMinimum_SetsFlag()
    {
        var participation = CreateParticipation();
        var day = ClosedDay(participation.Id, Tuesday, DayType.WeeklyMinimum, Debt.Zero);

        participation.RecalculateFrom(day);

        Assert.True(participation.WeeklyMinimumUsedThisWeek);
    }

    [Fact]
    public void RecalculateFrom_Monday_ResetsWeeklyMinimumFlag()
    {
        var participation = CreateParticipation();

        // Use the weekly minimum earlier in the week...
        var tuesday = ClosedDay(participation.Id, Tuesday, DayType.WeeklyMinimum, Debt.Zero);
        participation.RecalculateFrom(tuesday);
        Assert.True(participation.WeeklyMinimumUsedThisWeek);

        // ...then a new week begins on Monday with a regular day -> flag clears.
        var monday = ClosedDay(participation.Id, Monday, DayType.Regular, Debt.Zero);
        participation.RecalculateFrom(monday);

        Assert.False(participation.WeeklyMinimumUsedThisWeek);
    }

    [Fact]
    public void RecalculateFrom_WeeklyMinimumOnMonday_ResetsThenSetsFlagAgain()
    {
        var participation = CreateParticipation();
        var monday = ClosedDay(participation.Id, Monday, DayType.WeeklyMinimum, Debt.Zero);

        participation.RecalculateFrom(monday);

        Assert.True(participation.WeeklyMinimumUsedThisWeek);
    }

    [Fact]
    public void RecalculateFrom_OpenDay_Throws()
    {
        var participation = CreateParticipation();
        var openDay = DayRecord.Create(participation.Id, Tuesday, DayType.Regular);

        Assert.Throws<DomainException>(() => participation.RecalculateFrom(openDay));
    }

    [Fact]
    public void RecalculateFrom_DayFromAnotherParticipation_Throws()
    {
        var participation = CreateParticipation();
        var foreignDay = ClosedDay(Guid.NewGuid(), Tuesday, DayType.Regular, Debt.Zero);

        Assert.Throws<DomainException>(() => participation.RecalculateFrom(foreignDay));
    }
}
