using Stake.Domain.Common;

namespace Stake.Domain.ValueObjects;

public record Debt
{
    public TimeSpan Principal { get; }
    public TimeSpan Interest { get; }
    public TimeSpan Credit { get; }

    private Debt(TimeSpan principal, TimeSpan interest, TimeSpan credit)
    {
        Principal = principal;
        Interest = interest;
        Credit = credit;
    }

    public static Debt Zero { get; } = new(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

    public static Debt Create(TimeSpan principal, TimeSpan interest, TimeSpan credit)
    {
        if (principal < TimeSpan.Zero)
            throw new DomainException("Principal cannot be negative.");

        if (interest < TimeSpan.Zero)
            throw new DomainException("Interest cannot be negative.");

        if (credit < TimeSpan.Zero)
            throw new DomainException("Credit cannot be negative.");

        return new Debt(principal, interest, credit);
    }

    public Debt ApplyWork(TimeSpan worked, TimeSpan todayNorm, double penaltyPercent, PenaltyMode mode)
    {
        if (worked < TimeSpan.Zero)
            throw new DomainException("Worked time cannot be negative.");

        if (todayNorm < TimeSpan.Zero)
            throw new DomainException("Today's norm cannot be negative.");

        // Today's requirement joins the debt pool immediately, before any payment or interest.
        var principal = Principal + todayNorm;
        var interest = Interest;
        var credit = Credit;
        var remaining = worked;

        var interestPaid = Min(remaining, interest);
        interest -= interestPaid;
        remaining -= interestPaid;

        var principalPaid = Min(remaining, principal);
        principal -= principalPaid;
        remaining -= principalPaid;

        credit += remaining;

        // Interest is charged on whatever is still unpaid at the end of the day.
        var interestCharge = mode == PenaltyMode.Simple
            ? principal * penaltyPercent
            : (principal + interest) * penaltyPercent;
        interest += interestCharge;

        return Create(principal, interest, credit);
    }

    public Debt UseCredit(TimeSpan amount)
    {
        if (amount <= TimeSpan.Zero)
            throw new DomainException("Amount must be positive.");

        if (amount > Credit)
            throw new DomainException("Cannot use more credit than is available.");

        var totalDebt = Principal + Interest;
        var toSpend = Min(amount, totalDebt);

        var interestPaid = Min(toSpend, Interest);
        var interest = Interest - interestPaid;
        var remaining = toSpend - interestPaid;

        var principalPaid = Min(remaining, Principal);
        var principal = Principal - principalPaid;

        var credit = Credit - toSpend;

        return Create(principal, interest, credit);
    }

    private static TimeSpan Min(TimeSpan a, TimeSpan b) => a < b ? a : b;
}
