using System.Reflection;
using Application.AI;
using Application.AI.Memory;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(Behaviours.ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // Application-level abstractions (no concrete registrations here if they live in Infrastructure).
        return services;
    }
}

