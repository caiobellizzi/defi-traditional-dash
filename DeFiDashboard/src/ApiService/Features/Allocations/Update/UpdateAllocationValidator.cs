using FluentValidation;

namespace ApiService.Features.Allocations.Update;

public class UpdateAllocationValidator : AbstractValidator<UpdateAllocationCommand>
{
    public UpdateAllocationValidator()
    {
        RuleFor(x => x.Id)
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

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
