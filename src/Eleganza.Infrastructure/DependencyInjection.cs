using Eleganza.Application.Abstractions;
using Eleganza.Infrastructure.Data;
using Eleganza.Infrastructure.Identity;
using Eleganza.Infrastructure.Repositories;
using Eleganza.Infrastructure.Orders;
using Eleganza.Infrastructure.Shipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eleganza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        var security = configuration.GetSection("Security");
        var cookieName = security["CookieName"];
        if (string.IsNullOrWhiteSpace(cookieName))
        {
            throw new InvalidOperationException("Security:CookieName is required.");
        }

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = cookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = security.GetValue<bool>("RequireHttps")
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IVendorShippingAccountRepository, VendorShippingAccountRepository>();
        services.AddScoped<ISecretProtector, DataProtectionSecretProtector>();
        var dataProtection = configuration.GetSection("DataProtection");
        var keyRingPath = dataProtection["KeyRingPath"];
        var dataProtectionBuilder = services.AddDataProtection();
        if (!string.IsNullOrWhiteSpace(keyRingPath))
        {
            dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
        }
        else if (dataProtection.GetValue<bool>("RequirePersistentKeys"))
        {
            throw new InvalidOperationException("DataProtection:KeyRingPath is required when persistent keys are enabled.");
        }
        services.Configure<VanexOptions>(configuration.GetSection("Vanex"));
        services.AddHttpClient<IShippingProvider, VanexShippingProvider>((provider, client) =>
        {
            var seconds = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<VanexOptions>>().Value.HttpTimeoutSeconds;
            if (seconds <= 0)
            {
                throw new InvalidOperationException("Vanex:HttpTimeoutSeconds must be positive.");
            }

            client.Timeout = TimeSpan.FromSeconds(seconds);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false,
        });
        services.Configure<ShippingOptions>(configuration.GetSection("Shipping"));
        services.AddSingleton<IShippingFeeCalculator, FixedShippingFeeCalculator>();
        services.AddScoped<IIdentityRoleService, IdentityRoleService>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddHostedService<ShippingOutboxWorker>();

        return services;
    }
}
