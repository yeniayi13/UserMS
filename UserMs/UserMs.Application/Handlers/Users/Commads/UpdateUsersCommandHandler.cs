using UserMs.Application.Commands.User;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Infrastructure.Exceptions;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Newtonsoft.Json;
using UserMs.Core.RabbitMQ;
using UserMs.Core;
using UserMs.Infrastructure;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.UserEntity;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Application.Handlers.User.Commands
{
    [ExcludeFromCodeCoverage]
    public class UpdateUsersCommandHandler : IRequestHandler<UpdateUsersCommand,Users>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IKeycloakService _keycloakRepository;
        private readonly IEventBus<UpdateUsersDto> _eventBus;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IMapper _mapper;

        public UpdateUsersCommandHandler(IMapper mapper,IUserRepositoryMongo usersRepositoryMongo, IUserRepository usersRepository, IKeycloakService keycloakRepository, IEventBus<UpdateUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakRepository = keycloakRepository;
            _eventBus = eventBus;
            _usersRepositoryMongo = usersRepositoryMongo;
            _mapper = mapper;
        }

        public async Task<Users?> Handle(UpdateUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUsers = await _usersRepositoryMongo.GetUsersById(request.UserId.Value);

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
                    UserPassword.Create(existingUsers.UserPassword.Value),
                    UserName.Create(request.Users.UserName),
                    UserPhone.Create(request.Users.UserPhone),
                    UserAddress.Create(request.Users.UserAddress),
                    UserLastName.Create(request.Users.UserLastName),
                usersType,
                userAvailable
                );

                var update = _mapper.Map<UpdateUserDto>(users);
                // Actualizar el usuario en el repositorio
                _keycloakRepository.UpdateUser(existingUsers.UserId, update);
                await _usersRepository.UpdateUsersAsync(existingUsers.UserId, users);
                await _eventBus.PublishMessageAsync(request.Users, "userQueue", "USER_UPDATED");

                return users;
            }
            catch (UserNotFoundException ex)
            {
             
                throw;
            }
            catch (ArgumentException ex)
            {
             
                throw;
            }
            catch (Exception ex)
            {
              
                throw new ApplicationException("Ocurrió un error inesperado al actualizar el usuario.", ex);
            }
        }
    }
}