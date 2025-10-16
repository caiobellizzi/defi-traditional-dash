using FluentValidation;

namespace ApiService.Features.Allocations.Validate;

public class ValidateAllocationValidator : AbstractValidator<ValidateAllocationCommand>
{
    public ValidateAllocationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.AssetType)
            .NotEmpty()
            .Must(t => t == "Wallet" || t == "Account")
            .WithMessage("AssetType must be 'Wallet' or 'Account'");

        RuleFor(x => x.AssetId)
            .NotEmpty();

        RuleFor(x => x.AllocationType)
            .NotEmpty()
            .Must(t => t == "Percentage" || t == "FixedAmount")
            .WithMessage("AllocationType must be 'Percentage' or 'FixedAmount'");

        RuleFor(x => x.AllocationValue)
            .GreaterThan(0)
            .WithMessage("AllocationValue must be greater than 0");

        RuleFor(x => x.AllocationValue)
            .LessThanOrEqualTo(100)
            .When(x => x.AllocationType == "Percentage")
            .WithMessage("Percentage allocation must be between 0 and 100");

        RuleFor(x => x.StartDate)
            .NotEmpty();
    }
}
