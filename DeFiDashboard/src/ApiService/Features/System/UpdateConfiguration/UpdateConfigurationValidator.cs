using FluentValidation;

namespace ApiService.Features.System.UpdateConfiguration;

public class UpdateConfigurationValidator : AbstractValidator<UpdateConfigurationCommand>
{
    public UpdateConfigurationValidator()
    {
        RuleFor(x => x.Settings)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one setting must be provided");

        RuleForEach(x => x.Settings)
            .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
            .WithMessage("Configuration key cannot be empty");
    }
}
