using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : ICommand<Result<bool>>;
