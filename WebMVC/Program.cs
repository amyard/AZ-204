using Microsoft.FeatureManagement;
using WebMVC.Services;

var builder = WebApplication.CreateBuilder(args);

string connectionString = "Endpoint=https://cng-ene-dev-001.azconfig.io;Id=9xKl;Secret=iq0SHZd9QutmbNeIiqYyjQmdVQuxg7A6nbNt3gpxcWs=";
builder.Configuration.AddAzureAppConfiguration(
    options => options.Connect(connectionString).UseFeatureFlags());

builder.Services.AddTransient<IProductService, ProductService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddFeatureManagement();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();