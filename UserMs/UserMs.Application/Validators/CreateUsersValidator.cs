
using FluentValidation;
using UserMs.Common.Dtos.Users.Request;

namespace UserMs.Application.Validators
{
    public class CreateUsersValidator : ValidatorBase<CreateUsersDto>
    {
        public CreateUsersValidator()
        {
            RuleFor(s => s.UserEmail).NotNull().WithMessage("UserEmail no puede ser nulo").WithErrorCode("600");
            RuleFor(s => s.UsersType).NotNull().WithMessage("UsersType no puede ser nulo").WithErrorCode("600");
        }
    }
}