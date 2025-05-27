using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Keycloak;

namespace UserMs.Application.Commands.Keycloak
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public ResetPasswordDto ResetDto { get; set; }

        public ResetPasswordCommand(ResetPasswordDto resetDto)
        {
            ResetDto = resetDto;
        }
    }
}
