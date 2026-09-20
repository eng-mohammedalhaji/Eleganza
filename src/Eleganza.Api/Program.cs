using Eleganza.Api.Infrastructure;
using Eleganza.Application;
using Eleganza.Infrastructure;
using Eleganza.Infrastructure.Data;
using Eleganza.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
