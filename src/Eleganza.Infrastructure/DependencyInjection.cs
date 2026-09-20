using Eleganza.Application.Abstractions;
using Eleganza.Infrastructure.Data;
using Eleganza.Infrastructure.Identity;
using Eleganza.Infrastructure.Repositories;
using Eleganza.Infrastructure.Orders;
using Eleganza.Infrastructure.Shipping;
using Microsoft.AspNetCore.Identity;
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

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IVendorShippingAccountRepository, VendorShippingAccountRepository>();
        services.AddScoped<ISecretProtector, DataProtectionSecretProtector>();
        services.AddDataProtection();
        services.Configure<VanexOptions>(configuration.GetSection("Vanex"));
        services.AddHttpClient<IShippingProvider, VanexShippingProvider>();
        services.Configure<ShippingOptions>(configuration.GetSection("Shipping"));
        services.AddSingleton<IShippingFeeCalculator, FixedShippingFeeCalculator>();
        services.AddScoped<IIdentityRoleService, IdentityRoleService>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddHostedService<ShippingOutboxWorker>();

        return services;
    }
}
