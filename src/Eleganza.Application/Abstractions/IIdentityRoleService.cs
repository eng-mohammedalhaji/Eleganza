namespace Eleganza.Application.Abstractions;

public interface IIdentityRoleService
{
    Task AddToRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
}
