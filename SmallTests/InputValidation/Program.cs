using FluentValidation;
using InputValidation.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblies([typeof(Program).Assembly], includeInternalTypes: true);

builder.Services.Configure<ValidationSettings>(builder.Configuration.GetSection("ValidationSettings"));

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("api/register", (UserRegistrationDto registrationDto, IValidator<UserRegistrationDto> validator) =>
{
    var validationResult = validator.Validate(registrationDto);

    if (!validationResult.IsValid)
    {
        var problemDetails = new HttpValidationProblemDetails(validationResult.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Detail = "One or more validation errors occurred.",
            Instance = "api/register"
        };
        
        return Results.BadRequest(problemDetails);
    }
    
    return Results.Ok(new { Message = "Registration successful." });
});

app.UseHttpsRedirection();

app.Run();
