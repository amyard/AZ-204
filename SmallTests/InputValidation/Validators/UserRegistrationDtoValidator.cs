using System.Data;
using FluentValidation;
using InputValidation.Models;
using Microsoft.Extensions.Options;

namespace InputValidation.Validators;

internal sealed class UserRegistrationDtoValidator: AbstractValidator<UserRegistrationDto>
{
    public UserRegistrationDtoValidator(IOptions<ValidationSettings> options)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
        
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
        
        RuleFor(x => x.PersonalInfo)
            .NotNull().WithMessage("Personal info is required")
            .SetValidator(new PersonalInfoValidator());
        
        
        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address info is required")
            .SetValidator(new AddressInfoValidator());

        var minimumAge = options.Value.MinimumAge;
        RuleFor(x => x.DateOfBirth)
            .NotNull().WithMessage("Date of birth is required")
            .Must(BeInPast).WithMessage("Date of birth can't be in the future.")
            .Must(x => BeValidAge(x, minimumAge))
                .WithMessage($"You must be at least {minimumAge} years old.");
        
        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("You must accept terms and conditions.");
    }

    private static bool BeInPast(DateTime dateOfBirth)
    {
        return dateOfBirth < DateTime.Now;
    }

    private static bool BeValidAge(DateTime dateOfBirth, int minimumAge)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (DateTime.Today.AddYears(-age) < dateOfBirth)
        {
            age--;
        }
        return age >= minimumAge;
    }
    
}
