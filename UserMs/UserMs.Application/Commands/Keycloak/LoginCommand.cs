using MediatR;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Domain.Entities;

namespace UserMs.Application.Commands.Keycloak
{
    public class LoginCommand : IRequest<string>
    {
        public LoginDto Login { get; set; }

        public LoginCommand(LoginDto login)
        {
            Login = login;
        }
    }
}
