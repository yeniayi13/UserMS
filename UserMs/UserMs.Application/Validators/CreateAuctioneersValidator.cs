using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Application.Validators
{


    [ExcludeFromCodeCoverage]
    public class CreateAuctioneersValidator : ValidatorBase<CreateAuctioneerDto>
    {
        public CreateAuctioneersValidator()
        {
            RuleFor(x => x.UserEmail)
                .NotEmpty().WithMessage("El correo electrónico no puede estar vacío.")
                .EmailAddress().WithMessage("El correo electrónico no es válido.");

            RuleFor(x => x.UserPassword)
                .NotEmpty().WithMessage("La contraseña no puede estar vacía.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .MaximumLength(16).WithMessage("La contraseña no puede superar los 16 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe incluir al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe incluir al menos una letra minúscula.")
                .Matches(@"\d").WithMessage("La contraseña debe incluir al menos un número.")
                .Matches(@"[\W_]").WithMessage("La contraseña debe incluir al menos un carácter especial.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario no puede estar vacío.")
                .MaximumLength(50).WithMessage("El nombre de usuario no puede superar los 50 caracteres.");

            RuleFor(x => x.UserPhone)
                .NotEmpty().WithMessage("El número de teléfono no puede estar vacío.")
                .Matches(@"^\d{10,15}$").WithMessage("El número de teléfono debe contener entre 10 y 15 dígitos.");

            RuleFor(x => x.UserAddress)
                .NotEmpty().WithMessage("La dirección no puede estar vacía.")
                .MaximumLength(100).WithMessage("La dirección no puede superar los 100 caracteres.");

            RuleFor(x => x.UserLastName)
                .NotEmpty().WithMessage("El apellido no puede estar vacío.")
                .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres.");

            RuleFor(x => x.AuctioneerDni)
                .NotEmpty().WithMessage("El DNI del subastador no puede estar vacío.")
                .Length(8, 12).WithMessage("El DNI debe tener entre 8 y 12 caracteres.");

            RuleFor(x => x.AuctioneerBirthday)
                .NotEmpty().WithMessage("La fecha de nacimiento no puede estar vacía.")
                .Must(BeAValidDate).WithMessage("La fecha de nacimiento no es válida.")
                .Must(BeAnAdult).WithMessage("El subastador debe ser mayor de edad.");
        }

        private bool BeAValidDate(DateOnly birthday)
        {
            return birthday <= DateOnly.FromDateTime(DateTime.Now);
        }

        private bool BeAnAdult(DateOnly birthday)
        {
            return birthday <= DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
        }

    }
}
