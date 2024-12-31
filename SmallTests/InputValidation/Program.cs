using System.Threading.RateLimiting;
using FluentValidation;
using InputValidation.Extensions;
using InputValidation.Models;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblies([typeof(Program).Assembly], includeInternalTypes: true);

builder.Services.Configure<ValidationSettings>(builder.Configuration.GetSection("ValidationSettings"));

// option validation
builder.Services.AddOptions<GitHubSettings>()
    .BindConfiguration(GitHubSettings.ConfigurationSection)
    .ValidateFluentValidation()
    .ValidateOnStart();

// the same as on line 12 but as generic method
builder.Services.AddOptionsWithFluentValidation<GitHubSettings>(GitHubSettings.ConfigurationSection);

builder.Services.AddOpenApi();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    # region different rate algo
    // 10 request during 10 seconds
    // current approach is bad. All users can make only 10 requests 
    // options.AddFixedWindowLimiter("fixed" , o =>
    // {
    //     o.PermitLimit = 10;
    //     o.Window = TimeSpan.FromSeconds(10);
    // });

    // we have 3 section with 5s each (15 / 3 = 5s)
    // options.AddSlidingWindowLimiter("sliding", options =>
    // {
    //     options.Window = TimeSpan.FromSeconds(15);
    //     options.SegmentsPerWindow = 3;
    //     options.PermitLimit = 15;
    // });
    //
    // options.AddTokenBucketLimiter("token", options =>
    // {
    //     options.TokenLimit = 100;
    //     options.ReplenishmentPeriod = TimeSpan.FromSeconds(5);
    //     options.TokensPerPeriod = 10;
    // });
    //
    // options.AddConcurrencyLimiter("concurrency", options =>
    // {
    //     options.PermitLimit = 5;
    // });
    # endregion
    
    // request depends on IP address
    options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            partitionKey: httpContext.User.Identity?.Name?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10)
            }));
});


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
})
.RequireRateLimiting("fixed");

app.MapPost("api/v2/register", (UserRegistrationDto registrationDto, IValidator<UserRegistrationDto> validator) =>
{
    return Results.Ok(new { Message = "Registration successful." });
})
.DisableRateLimiting();


app.UseHttpsRedirection();

app.UseRateLimiter();


app.Run();
