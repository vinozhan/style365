using Style365.Domain.Entities;
using Style365.Domain.ValueObjects;

namespace Style365.Application.Common.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User?> GetByCognitoUserIdAsync(string cognitoUserId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithAddressesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithOrdersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(Email email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(Domain.Enums.UserRole role, CancellationToken cancellationToken = default);
}