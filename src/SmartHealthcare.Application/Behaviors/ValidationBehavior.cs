using FluentValidation;
using MediatR;

namespace SmartHealthcare.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this.validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResult = await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failure = validationResult
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failure.Any())
                {
                    throw new ValidationException(failure);
                }
            }

            return await next();
        }
    }
}