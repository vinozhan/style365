using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.UserProfiles.Queries.GetProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserProfileHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(request.UserId, cancellationToken);
        
        if (profile == null)
        {
            throw new NotFoundException(nameof(UserProfile), request.UserId);
        }

        var profileDto = new UserProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            FullName = profile.GetFullName(),
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Gender = profile.Gender,
            ProfileImageUrl = profile.ProfileImageUrl,
            EmailNotifications = profile.EmailNotifications,
            SmsNotifications = profile.SmsNotifications,
            PreferredLanguage = profile.PreferredLanguage ?? "en",
            PreferredCurrency = profile.PreferredCurrency ?? "USD",
            FavoriteCategories = profile.GetFavoriteCategories(),
            PreferredBrands = profile.GetPreferredBrands(),
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };

        return Result.Success(profileDto);
    }
}