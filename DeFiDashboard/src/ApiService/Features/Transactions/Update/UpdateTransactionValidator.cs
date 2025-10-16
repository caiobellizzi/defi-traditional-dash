using FluentValidation;

namespace ApiService.Features.Transactions.Update;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Direction)
            .NotEmpty()
            .Must(d => d == "IN" || d == "OUT" || d == "INTERNAL")
            .WithMessage("Direction must be 'IN', 'OUT', or 'INTERNAL'");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Category)
            .MaximumLength(50);

        RuleFor(x => x.TokenSymbol)
            .MaximumLength(20);

        RuleFor(x => x.Reason)
            .MaximumLength(500);
    }
}
