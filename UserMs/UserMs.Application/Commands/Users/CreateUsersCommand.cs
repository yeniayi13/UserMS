
using MediatR;
using UserMs.Domain.Entities;
using UserMs.Common.Dtos.Users.Request;

namespace UserMs.Application.Commands.User
{
    public class CreateUsersCommand : IRequest<UserId>
    {
        public CreateUsersDto Users { get; set; }

        public CreateUsersCommand(CreateUsersDto users)
        {
            Users = users;
        }
    }
}