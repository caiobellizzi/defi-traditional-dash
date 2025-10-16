using FluentValidation;

namespace ApiService.Features.Wallets.Update;

public class UpdateWalletValidator : AbstractValidator<UpdateWalletCommand>
{
    public UpdateWalletValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Wallet ID is required");

        RuleFor(x => x.Label)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Label))
            .WithMessage("Label must not exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must not exceed 2000 characters");
    }
}
