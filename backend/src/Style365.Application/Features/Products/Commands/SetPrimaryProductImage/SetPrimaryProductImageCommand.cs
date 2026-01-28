using Style365.Application.Common.Interfaces;
using Style365.Application.Common.Models;

namespace Style365.Application.Features.Products.Commands.SetPrimaryProductImage;

public record SetPrimaryProductImageCommand : ICommand<Result>
{
    public Guid ProductId { get; init; }
    public Guid ImageId { get; init; }
}
