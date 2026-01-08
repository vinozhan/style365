using AutoMapper;
using MediatR;
using Style365.Application.Common.DTOs;
using Style365.Application.Common.Exceptions;
using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;
using Style365.Domain.Entities;

namespace Style365.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdWithFullDetailsAsync(request.Id, cancellationToken);
        
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        var productDto = _mapper.Map<ProductDto>(product);
        
        return Result.Success(productDto);
    }
}