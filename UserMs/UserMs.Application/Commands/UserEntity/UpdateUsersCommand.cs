
using MediatR;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Domain.Entities.UserEntity;


namespace Commands.UserEntity
{
    public class UpdateUsersCommand :IRequest<Users>
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