using FluentValidation;

namespace ApiService.Features.Accounts.Update;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.Label)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Label))
            .WithMessage("Label must not exceed 200 characters");
    }
}
