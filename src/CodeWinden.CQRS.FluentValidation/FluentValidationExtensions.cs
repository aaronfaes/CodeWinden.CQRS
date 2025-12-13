using CodeWinden.CQRS;

namespace CodeWinden.CQRS.FluentValidation;

public static class FluentValidationExtensions
{
    extension(CQRSOptionsBuilder builder)
    {
        public CQRSOptionsBuilder AddFluentValidation()
        {
            // Add Decorators for FluentValidation
            // builder.AddDecorator<FluentValidationCommandHandlerDecorator>();
            // builder.AddDecorator<FluentValidationCommandWithResultHandlerDecorator>();
            // builder.AddDecorator<FluentValidationQueryHandlerDecorator>();
            // builder.AddDecorator<FluentValidationParameterlessQueryHandlerDecorator>();

            return builder;
        }
    }
}