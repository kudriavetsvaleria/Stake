using Stake.Domain.Common;
using Stake.Domain.Entities;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Tests;

public class DayRecordTests
{
    private static readonly DateOnly SomeDate = new(2026, 1, 1);

    [Fact]
    public void Create_StartsOpenRegularDayWithNoWorkAndNoSnapshot()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);

        Assert.NotEqual(Guid.Empty, day.Id);
        Assert.Equal(SomeDate, day.Date);
        Assert.Equal(DayType.Regular, day.Type);
        Assert.Equal(TimeSpan.Zero, day.TotalWorked);
        Assert.Null(day.DebtSnapshot);
        Assert.False(day.IsClosed);
    }

    [Fact]
    public void AddWork_AccumulatesTotalWorked()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);

        day.AddWork(TimeSpan.FromHours(1));
        day.AddWork(TimeSpan.FromMinutes(30));

        Assert.Equal(TimeSpan.FromMinutes(90), day.TotalWorked);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddWork_WithNonPositiveTime_Throws(int hours)
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);

        Assert.Throws<DomainException>(() => day.AddWork(TimeSpan.FromHours(hours)));
    }

    [Fact]
    public void SetType_ChangesTypeOnOpenDay()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);

        day.SetType(DayType.DayOff);

        Assert.Equal(DayType.DayOff, day.Type);
    }

    [Fact]
    public void Close_StoresSnapshotAndMarksClosed()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);
        var finalDebt = Debt.Create(TimeSpan.FromHours(2), TimeSpan.Zero, TimeSpan.Zero);

        day.Close(finalDebt);

        Assert.True(day.IsClosed);
        Assert.Equal(finalDebt, day.DebtSnapshot);
    }

    [Fact]
    public void AddWork_OnClosedDay_Throws()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);
        day.Close(Debt.Zero);

        Assert.Throws<DomainException>(() => day.AddWork(TimeSpan.FromHours(1)));
    }

    [Fact]
    public void SetType_OnClosedDay_Throws()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);
        day.Close(Debt.Zero);

        Assert.Throws<DomainException>(() => day.SetType(DayType.DayOff));
    }

    [Fact]
    public void Close_WhenAlreadyClosed_Throws()
    {
        var day = DayRecord.Create(Guid.NewGuid(), SomeDate);
        day.Close(Debt.Zero);

        Assert.Throws<DomainException>(() => day.Close(Debt.Zero));
    }
}
