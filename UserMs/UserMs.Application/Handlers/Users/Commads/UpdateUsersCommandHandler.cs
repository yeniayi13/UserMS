using UserMs.Application.Commands.User;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Infrastructure.Exceptions;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Newtonsoft.Json;
using UserMs.Core.RabbitMQ;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Core;
using UserMs.Infrastructure;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Commoon.Dtos;

namespace UserMs.Application.Handlers.User.Commands
{
    public class UpdateUsersCommandHandler : IRequestHandler<UpdateUsersCommand,Users>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IKeycloakRepository _keycloakRepository;
        private readonly IEventBus<UpdateUsersDto> _eventBus;

        public UpdateUsersCommandHandler(IUserRepository usersRepository, IKeycloakRepository keycloakRepository, IEventBus<UpdateUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakRepository = keycloakRepository;
            _eventBus = eventBus;
        }

        public async Task<Users?> Handle(UpdateUsersCommand request, CancellationToken cancellationToken)
        {
            var existingUsers = await _usersRepository.GetUsersById(request.UserId.Value);

            if (existingUsers == null)
                throw new UserNotFoundException("User not found.");

            // Validación adicional en `UsersType`
            if (!Enum.TryParse<UsersType>(request.Users.UsersType.ToString(), out var usersType) ||
                !Enum.TryParse<UserAvailable>(request.Users.UserAvailable.ToString(), out var userAvailable))
            {
                throw new ArgumentException("Invalid UsersType or UserAvailable");
            }

            var users = new Users(
                UserId.Create(request.UserId.Value),
                UserEmail.Create(request.Users.UserEmail),
                UserPassword.Create(request.Users.UserPassword),
                UserName.Create(request.Users.UserName),
                UserPhone.Create(request.Users.UserPhone),
                UserAddress.Create(request.Users.UserAddress),
                UserLastName.Create(request.Users.UserLastName),
                usersType,
                userAvailable
            );

            var update = new UpdateUserDto
            {
                //UserId = request.UserId, // Asegurar que sea un `Guid`
                UserEmail = request.Users.UserEmail,
                UserPassword = request.Users.UserPassword,
                UserName = request.Users.UserName,
                UserLastName = request.Users.UserLastName,
                UserPhone = request.Users.UserPhone,
                UserAddress = request.Users.UserAddress,
            };


            // Actualizar el usuario en el repositorio
            //_keycloakRepository.UpdateUser(existingUsers.UserId, update);
            await _usersRepository.UpdateUsersAsync(existingUsers.UserId, users);
            await _eventBus.PublishMessageAsync(request.Users, "userQueue", "USER_DELETED");
            return users;
        }
    }
}