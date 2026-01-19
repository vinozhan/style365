using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Style365.Application.Common.Behaviours;

namespace Style365.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}