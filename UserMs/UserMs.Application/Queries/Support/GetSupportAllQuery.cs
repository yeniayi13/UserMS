using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;

namespace UserMs.Application.Queries.Support
{
    public class GetSupportAllQuery : IRequest<List<GetSupportDto>>
    {
        public Guid SupportId { get; private set; }
        public Guid UserId { get; private set; }
        public string SupportDni { get; private set; }
        public string SupportSpecialization { get; private set; }
    }
}
