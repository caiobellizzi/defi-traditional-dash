using FluentValidation;

namespace ApiService.Features.Transactions.Categorize;

public class CategorizeTransactionValidator : AbstractValidator<CategorizeTransactionCommand>
{
    public CategorizeTransactionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Category)
            .MaximumLength(50);
    }
}
