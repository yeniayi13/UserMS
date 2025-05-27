
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Validators
{

    [ExcludeFromCodeCoverage]
    public class ValidatorBase<T> : AbstractValidator<T>
    {
        public virtual async Task<bool> ValidateRequest(T request)
        {
            var result = await ValidateAsync(request);
            if (!result.IsValid)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new ValidatorException(errorMessage);
            }

            return result.IsValid;
        }
    }
}