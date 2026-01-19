using System.Linq.Expressions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;
using Style365.Domain.Enums;

namespace Style365.Application.Features.Users.Queries.GetCustomers;

public class GetCustomersHandler : IQueryHandler<GetCustomersQuery, Result<PaginatedResult<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResult<CustomerDto>>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Build predicate for filtering customers
            Expression<Func<User, bool>> predicate = u => u.Role == UserRole.Customer;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                predicate = u => u.Role == UserRole.Customer &&
                    (u.FirstName.ToLower().Contains(searchTerm) ||
                     u.LastName.ToLower().Contains(searchTerm) ||
                     u.Email.Value.ToLower().Contains(searchTerm));
            }

            // Get total count
            var totalCount = await _unitOfWork.Users.CountAsync(predicate, cancellationToken);

            // Get paginated customers
            var (customers, _) = await _unitOfWork.Users.GetPagedAsync(
                request.Page,
                request.PageSize,
                predicate,
                GetSortExpression(request.SortBy),
                request.Ascending,
                cancellationToken);

            // Map to DTOs with order statistics
            var customerDtos = new List<CustomerDto>();
            foreach (var customer in customers)
            {
                var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(customer.Id, cancellationToken);
                var ordersList = orders.ToList();

                customerDtos.Add(new CustomerDto
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email.Value,
                    PhoneNumber = customer.PhoneNumber,
                    IsActive = customer.IsActive,
                    OrdersCount = ordersList.Count,
                    TotalSpent = ordersList.Sum(o => o.TotalAmount.Amount),
                    LastOrderDate = ordersList.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt,
                    CreatedAt = customer.CreatedAt
                });
            }

            var result = PaginatedResult<CustomerDto>.Create(
                customerDtos,
                request.Page,
                request.PageSize,
                totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<PaginatedResult<CustomerDto>>($"Failed to retrieve customers: {ex.Message}");
        }
    }

    private static Expression<Func<User, object>>? GetSortExpression(string? sortBy)
    {
        return sortBy?.ToLower() switch
        {
            "firstname" or "name" => u => u.FirstName,
            "lastname" => u => u.LastName,
            "email" => u => u.Email.Value,
            "createdat" or "joined" => u => u.CreatedAt,
            _ => u => u.CreatedAt
        };
    }
}
