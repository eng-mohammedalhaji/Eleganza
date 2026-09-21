using Eleganza.Application.Vendors;
using Eleganza.Application.Catalog;
using Eleganza.Application.Orders;
using Eleganza.Application.Shipping;
using Microsoft.Extensions.DependencyInjection;

namespace Eleganza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<VendorService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<ProductService>();
        services.AddScoped<OrderService>();
        services.AddScoped<VendorShippingAccountService>();
        services.AddScoped<VanexLocationService>();
        return services;
    }
}
