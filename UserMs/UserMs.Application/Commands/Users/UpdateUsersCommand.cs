
using MediatR;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Request;

namespace UserMs.Application.Commands.User
{
    public class UpdateUsersCommand : IRequest<Users>
    {
        public UserId UserId { get; set; }
        public UpdateUsersDto Users { get; set; }

        public UpdateUsersCommand(UserId userId, UpdateUsersDto users)
        {
            UserId = userId;
            Users = users;
        }
    }
}