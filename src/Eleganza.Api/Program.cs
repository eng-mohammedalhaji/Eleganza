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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
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
