using AutoMapper;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Users.Queries.GetUser;

public class GetUserHandler : IQueryHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
            {
                return Result.Failure<UserDto>("User not found");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return Result.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDto>($"Failed to retrieve user: {ex.Message}");
        }
    }
}