using Style365.Domain.Entities;

namespace Style365.Application.Common.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUserIdWithAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProfile>> GetProfilesByPreferredCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProfile>> GetProfilesByPreferredBrandAsync(string brand, CancellationToken cancellationToken = default);
}