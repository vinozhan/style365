using MediatR;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;