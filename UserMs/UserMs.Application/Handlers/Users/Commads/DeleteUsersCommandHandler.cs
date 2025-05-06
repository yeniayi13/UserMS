using UserMs.Application.Commands.User;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Core.RabbitMQ;
using UserMs.Core;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Application.Dtos.Users.Response;

namespace UserMs.Application.Handlers.User.Commands
{
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand, UserId>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IKeycloakRepository _keycloakRepository;
        private readonly IEventBus<GetUsersDto> _eventBus;

        public DeleteUsersCommandHandler(IUserRepository usersRepository, IKeycloakRepository keycloakRepository, IEventBus<GetUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakRepository = keycloakRepository;
            _eventBus = eventBus;
        }

        public async Task<UserId> Handle(DeleteUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _usersRepository.GetUsersById(request.UserId);
               // var deleted = await _keycloakRepository.DeleteUserAsync(users!.UserId);
                var userEntity = new Users(
                   UserId.Create(users.UserId),
                   //UserId.Create(Id),
                   UserEmail.Create(users.UserEmail),
                   UserPassword.Create(users.UserPassword),
                   UserName.Create(users.UserName),
                   UserPhone.Create(users.UserPhone),
                   UserAddress.Create(users.UserAddress),
                   UserLastName.Create(users.UserLastName),
                   Enum.Parse<UsersType>(users.UsersType),
                   Enum.Parse<UserAvailable>(users.UserAvailable),
                   UserDelete.Create(users.UserDelete)
               );
                userEntity.SetUserDelete(UserDelete.Create(true));
                await _usersRepository.DeleteUsersAsync(request.UserId);
                await _eventBus.PublishMessageAsync(users, "userQueue", "USER_DELETED");
                return request.UserId;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}