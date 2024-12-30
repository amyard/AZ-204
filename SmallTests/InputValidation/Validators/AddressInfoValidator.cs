using FluentValidation;
using InputValidation.Models;

namespace InputValidation.Validators;

internal sealed class AddressInfoValidator: AbstractValidator<AddressInfo>
{
    public AddressInfoValidator()
    {
        RuleFor(x => x.Street).NotEmpty().WithMessage("Street is required");
        RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
        RuleFor(x => x.State).NotEmpty().WithMessage("State is required");
        RuleFor(x => x.PostalCode).NotEmpty().WithMessage("PostalCode is required");
        RuleFor(x => x.Country).NotEmpty().WithMessage("Country is required");
    }
}
