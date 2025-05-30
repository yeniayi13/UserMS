using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Infrastructure.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class ExceptionHandlerService
    {
        public static void HandleException(Exception ex)
        {
            switch (ex)
            {
                case ValidationException:
                    throw new ValidationException("Error de validación: " + ex.Message);
                case UserNotFoundException:
                    throw new UserNotFoundException("Usuario no encontrado.");
                case RoleNotFoundException:
                    throw new RoleNotFoundException("Rol no encontrado.");
                case UserRoleExistException:
                    throw new UserRoleExistException("Este usuario ya tiene este rol.");
                default:
                    throw new ApplicationException("Ocurrió un error inesperado.", ex);
            }
        }
    }
}
