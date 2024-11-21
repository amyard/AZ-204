using SemanticSearch.API.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AzureConfigs>(
    builder.Configuration.GetSection(nameof(AzureConfigs)));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
