using MediatR;

namespace Style365.Application.Common.Interfaces;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}