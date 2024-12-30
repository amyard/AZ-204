using FluentValidation;
using InputValidation.Models;

namespace InputValidation.Validators;

internal sealed class PersonalInfoValidator: AbstractValidator<PersonalInfo>
{
    public PersonalInfoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
    }
}
