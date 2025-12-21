using CodeWinden.CQRS.FluentValidation.Decorators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CodeWinden.CQRS;

/// <summary>
/// Extension methods to add FluentValidation decorators to CQRS pipeline.
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation decorators to the CQRS pipeline.
    /// </summary>
    /// <returns>The updated CQRSOptionsBuilder.</returns>
    public static CQRSOptionsBuilder AddFluentValidation<TAssembly>(this CQRSOptionsBuilder builder)
    {
        // Add Decorators for FluentValidation
        builder.AddDecorator(typeof(FluentValidationCommandHandlerDecorator<>));
        builder.AddDecorator(typeof(FluentValidationCommandWithResultHandlerDecorator<,>));
        builder.AddDecorator(typeof(FluentValidationQueryHandlerDecorator<,>));

        // Register all AbstractValidator<> types from the assemblies
        builder.AddAdditionalRegistration(services => services.AddValidatorsFromAssemblyContaining<TAssembly>(ServiceLifetime.Scoped));

        return builder;
    }
}
