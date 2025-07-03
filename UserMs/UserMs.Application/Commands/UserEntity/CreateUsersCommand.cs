
using MediatR;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Request.User;

namespace UserMs.Application.Commands.UserEntity
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