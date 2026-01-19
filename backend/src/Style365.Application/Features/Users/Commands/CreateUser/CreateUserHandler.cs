using AutoMapper;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.ValueObjects;

namespace Style365.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUserHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email is already taken
            var email = Email.Create(request.Email);
            var emailExists = !await _unitOfWork.Users.IsEmailUniqueAsync(email, cancellationToken: cancellationToken);
            
            if (emailExists)
            {
                return Result.Failure<UserDto>("Email address is already registered");
            }

            // Create new user
            var user = new User(request.FirstName, request.LastName, request.Email, request.Role);
            
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) || request.DateOfBirth.HasValue)
            {
                user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, request.DateOfBirth);
            }

            if (!string.IsNullOrWhiteSpace(request.CognitoUserId))
            {
                user.SetCognitoUserId(request.CognitoUserId);
            }

            // Add to repository
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO and return
            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDto>($"Failed to create user: {ex.Message}");
        }
    }
}