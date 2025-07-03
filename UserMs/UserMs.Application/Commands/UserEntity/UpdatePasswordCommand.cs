using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Keycloak;

namespace UserMs.Application.Commands.Users
{
    public class UpdatePasswordCommand: IRequest<Guid>
    {
       public UpdatePasswordDto UpdatePasswordDto { get; set; }
        public Guid UserId { get; set; }

        public UpdatePasswordCommand(UpdatePasswordDto updatePasswordDto , Guid userId)
        {
            UpdatePasswordDto = updatePasswordDto;
            UserId = userId;
        }
    }
}
