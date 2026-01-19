using MediatR;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.UserProfiles.Commands.UpdateProfile;

public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserProfileHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Get existing profile or create new one
        var profile = await _unitOfWork.UserProfiles.GetByUserIdAsync(request.UserId, cancellationToken);
        
        if (profile == null)
        {
            // Create new profile
            profile = new UserProfile(request.UserId, request.FirstName, request.LastName);
            await _unitOfWork.UserProfiles.AddAsync(profile, cancellationToken);
        }
        else
        {
            // Update existing profile
            profile.UpdateBasicInfo(request.FirstName, request.LastName, request.PhoneNumber);
        }

        // Update personal information
        profile.UpdatePersonalInfo(request.DateOfBirth, request.Gender);

        // Update notification preferences
        profile.UpdateNotificationPreferences(request.EmailNotifications, request.SmsNotifications);

        // Update language preferences
        profile.UpdateLanguagePreferences(request.PreferredLanguage, request.PreferredCurrency);

        // Update preferences
        profile.UpdateFavoriteCategories(request.FavoriteCategories);
        profile.UpdatePreferredBrands(request.PreferredBrands);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}