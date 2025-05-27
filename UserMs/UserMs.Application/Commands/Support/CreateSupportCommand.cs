using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Application.Commands.Support
{
    public class CreateSupportCommand : IRequest<UserId>
    {
        public CreateSupportDto Support { get; set; }

        public CreateSupportCommand(CreateSupportDto support)
        {
            Support = support;
        }
    }
}
