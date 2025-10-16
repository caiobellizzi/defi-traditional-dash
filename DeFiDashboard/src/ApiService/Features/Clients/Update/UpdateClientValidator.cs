using FluentValidation;

namespace ApiService.Features.Clients.Update;

public class UpdateClientValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Client ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Document)
            .MaximumLength(50).WithMessage("Document must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Document));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Status)
            .Must(s => s is null or "Active" or "Inactive" or "Suspended")
            .WithMessage("Status must be Active, Inactive, or Suspended")
            .When(x => x.Status != null);
    }
}
