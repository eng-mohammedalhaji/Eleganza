using Eleganza.Api.Infrastructure;
using Eleganza.Application;
using Eleganza.Infrastructure;
using Eleganza.Infrastructure.Data;
using Eleganza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.Configure<RateLimitOptions>(builder.Configuration.GetSection("RateLimits"));
var rateLimitOptions = builder.Configuration.GetSection("RateLimits").Get<RateLimitOptions>()
    ?? throw new InvalidOperationException("RateLimits configuration is required.");
if (rateLimitOptions.AuthPermitLimit <= 0
    || rateLimitOptions.AuthWindowSeconds <= 0
    || rateLimitOptions.VendorLocationPermitLimit <= 0
    || rateLimitOptions.VendorLocationWindowSeconds <= 0)
{
    throw new InvalidOperationException("All RateLimits values must be positive.");
}
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitOptions.AuthPermitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitOptions.AuthWindowSeconds);
        limiterOptions.QueueLimit = 0;
        limiterOptions.AutoReplenishment = true;
    });
    options.AddFixedWindowLimiter("vendor-locations", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitOptions.VendorLocationPermitLimit;
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitOptions.VendorLocationWindowSeconds);
        limiterOptions.QueueLimit = 0;
        limiterOptions.AutoReplenishment = true;
    });
});
builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Eleganza.Application.Abstractions.ICurrentUser, HttpCurrentUser>();
builder.Services.AddAuthorization();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services, builder.Configuration, app.Environment);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
var authGroup = app.MapGroup("/api/auth").RequireRateLimiting("auth");
authGroup.MapIdentityApi<ApplicationUser>();
authGroup.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
