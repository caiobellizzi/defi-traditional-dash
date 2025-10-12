using FluentValidation;

namespace ApiService.Features.Allocations.Create;

public class CreateAllocationValidator : AbstractValidator<CreateAllocationCommand>
{
    public CreateAllocationValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required")
            .Must(t => t is "Wallet" or "Account")
            .WithMessage("Asset type must be 'Wallet' or 'Account'");

        RuleFor(x => x.AssetId)
            .NotEmpty().WithMessage("Asset ID is required");

        RuleFor(x => x.AllocationType)
            .NotEmpty().WithMessage("Allocation type is required")
            .Must(t => t is "Percentage" or "FixedAmount")
            .WithMessage("Allocation type must be 'Percentage' or 'FixedAmount'");

        RuleFor(x => x.AllocationValue)
            .GreaterThan(0).WithMessage("Allocation value must be greater than 0")
            .LessThanOrEqualTo(100)
            .When(x => x.AllocationType == "Percentage")
            .WithMessage("Percentage allocation must be between 0 and 100");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");
    }
}
