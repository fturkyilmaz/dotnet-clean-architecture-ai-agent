using Application.Common.Results;
using FluentValidation;
using MediatR;

namespace Application.Behaviours;

public sealed class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        // Result<T> ile uyumlu olacak şekilde Validation error döndürmeye çalış.
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var error = Common.Errors.Error.Validation(
                "Validation.Failed",
                "One or more validation errors occurred.",
                string.Join("; ", failures.Select(f => f.ErrorMessage)));

            var resultType = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0]);
            var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure))!;
            return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
        }

        // Fallback: normal akışı bozma, handler'a bırak.
        return await next();
    }
}

