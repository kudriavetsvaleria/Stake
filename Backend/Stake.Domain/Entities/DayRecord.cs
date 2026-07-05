using Stake.Domain.Common;
using Stake.Domain.ValueObjects;

namespace Stake.Domain.Entities;

public class DayRecord
{
    public Guid Id { get; private set; }
    public Guid ChallengeParticipationId { get; private set; }
    public DateOnly Date { get; private set; }
    public DayType Type { get; private set; }
    public TimeSpan TotalWorked { get; private set; }
    public Debt? DebtSnapshot { get; private set; }
    public bool IsClosed { get; private set; }

    private DayRecord(
        Guid id,
        Guid challengeParticipationId,
        DateOnly date,
        DayType type,
        TimeSpan totalWorked,
        Debt? debtSnapshot,
        bool isClosed)
    {
        Id = id;
        ChallengeParticipationId = challengeParticipationId;
        Date = date;
        Type = type;
        TotalWorked = totalWorked;
        DebtSnapshot = debtSnapshot;
        IsClosed = isClosed;
    }

    public static DayRecord Create(Guid challengeParticipationId, DateOnly date, DayType type = DayType.Regular)
    {
        return new DayRecord(
            Guid.NewGuid(),
            challengeParticipationId,
            date,
            type,
            TimeSpan.Zero,
            debtSnapshot: null,
            isClosed: false);
    }

    public void AddWork(TimeSpan worked)
    {
        if (IsClosed)
            throw new DomainException("Cannot add work to a closed day.");

        if (worked <= TimeSpan.Zero)
            throw new DomainException("Worked time must be positive.");

        TotalWorked += worked;
    }

    public void SetType(DayType type)
    {
        if (IsClosed)
            throw new DomainException("Cannot change the type of a closed day.");

        Type = type;
    }

    public void Close(Debt finalDebt)
    {
        if (IsClosed)
            throw new DomainException("The day is already closed.");

        DebtSnapshot = finalDebt;
        IsClosed = true;
    }
}
