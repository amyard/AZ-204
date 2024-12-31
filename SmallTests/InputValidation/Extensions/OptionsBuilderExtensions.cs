using InputValidation.Validators;
using Microsoft.Extensions.Options;

namespace InputValidation.Extensions;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluentValidation<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
    {
        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
            provider => new FluentValidateOptions<TOptions>(provider, optionsBuilder.Name));
        
        return optionsBuilder;
    }
}
