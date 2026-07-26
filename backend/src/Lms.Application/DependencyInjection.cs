using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Lms.Application;

/// <summary>Registers Application-layer services: validators and AutoMapper profiles.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Transient);
        services.AddAutoMapper(assembly);

        return services;
    }
}
