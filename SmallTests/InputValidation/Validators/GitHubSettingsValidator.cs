using FluentValidation;
using InputValidation.Models;

namespace InputValidation.Validators;

public sealed class GitHubSettingsValidator : AbstractValidator<GitHubSettings>
{
    public GitHubSettingsValidator()
    {
        RuleFor(x => x.AccessToken).NotNull().NotEmpty();
        RuleFor(x => x.UserAgent).NotNull().NotEmpty();
        RuleFor(x => x.BaseAddress).NotNull().NotEmpty();
        
        // this rule will be called if BaseAddress is not null
        RuleFor(x => x.BaseAddress)
            .Must(baseAddress => Uri.TryCreate(baseAddress, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.BaseAddress))
            .WithMessage(x => "Invalid GitHub API address");
    }
}
