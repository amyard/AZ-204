using FluentValidation;
using Microsoft.Extensions.Options;

namespace InputValidation.Validators;

// WE are using it for GitHubSettings option validation
public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _name;

    public FluentValidateOptions(IServiceProvider serviceProvider, string? name)
    {
        _serviceProvider = serviceProvider;
        _name = name;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        if (_name is not null && _name != name)
        {
            return ValidateOptionsResult.Skip;
        }
        
        ArgumentNullException.ThrowIfNull(options);
        
        using var scope = _serviceProvider.CreateScope();
        var validators = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();
        var result = validators.Validate(options);

        if (result.IsValid)
        {
            return ValidateOptionsResult.Success;
        }
        
        var type = options.GetType().Name;
        // var errors = result.Errors.Select(e => e.ErrorMessage);
        var errors = result.Errors.Select(e => $"Validation failed for {type}.{e.PropertyName} with the error: {e.ErrorMessage}");
        
        return ValidateOptionsResult.Fail(errors);
    }
}
