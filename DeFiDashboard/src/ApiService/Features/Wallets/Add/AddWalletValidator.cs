using FluentValidation;

namespace ApiService.Features.Wallets.Add;

public class AddWalletValidator : AbstractValidator<AddWalletCommand>
{
    public AddWalletValidator()
    {
        RuleFor(x => x.WalletAddress)
            .NotEmpty().WithMessage("Wallet address is required")
            .MaximumLength(100).WithMessage("Wallet address must not exceed 100 characters")
            .Matches("^0x[a-fA-F0-9]{40}$").WithMessage("Invalid Ethereum wallet address format")
            .When(x => !string.IsNullOrEmpty(x.WalletAddress));

        RuleFor(x => x.Label)
            .MaximumLength(200).WithMessage("Label must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Label));
    }
}
