using Eleganza.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Eleganza.Infrastructure.Identity;

public sealed class IdentityRoleService(UserManager<ApplicationUser> userManager) : IIdentityRoleService
{
    public async Task AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new KeyNotFoundException("The user for this vendor was not found.");
        }

        if (await userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not assign role {role}: {errors}");
        }
    }
}
