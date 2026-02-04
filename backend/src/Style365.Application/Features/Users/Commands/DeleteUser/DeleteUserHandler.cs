using MediatR;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authService;

    public DeleteUserHandler(IUnitOfWork unitOfWork, IAuthenticationService authService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Get the user from database
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        // Prevent deletion of admin users for safety
        if (user.Role == UserRole.Admin)
        {
            return Result.Failure("Cannot delete admin users. Please demote the user first.");
        }

        // Delete from Cognito first (if user has a Cognito account)
        if (!string.IsNullOrEmpty(user.CognitoUserId))
        {
            var cognitoResult = await _authService.DeleteUserAsync(user.CognitoUserId);
            if (!cognitoResult.IsSuccess)
            {
                return Result.Failure($"Failed to delete user from authentication provider: {string.Join(", ", cognitoResult.Errors)}");
            }
        }

        // Soft delete from database
        user.MarkAsDeleted();
        user.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
