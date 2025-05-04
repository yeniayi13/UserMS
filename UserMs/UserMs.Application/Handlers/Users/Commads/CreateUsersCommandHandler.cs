using UserMs.Application.Commands.User;
using UserMs.Application.Validators;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Core;
using UserMs.Common.Dtos.Users.Request;
using UserMs.Core.RabbitMQ;
using UserMs.Commoon.Dtos;


namespace UserMs.Application.Handlers.User.Commands
{
    public class CreateUsersCommandHandler : IRequestHandler<CreateUsersCommand, UserId>
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IKeycloakRepository _keycloakMsService;
        private readonly IEventBus<CreateUsersDto> _eventBus;

        public CreateUsersCommandHandler(IUsersRepository usersRepository, IKeycloakRepository keycloakMsService, IEventBus<CreateUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakMsService = keycloakMsService;
            _eventBus = eventBus;
        }



        public async Task<UserId> Handle(CreateUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var validator = new CreateUsersValidator();
                await validator.ValidateRequest(request.Users);
                //var userId = request.Users.UserId;
                var usersEmailValue = request.Users.UserEmail;
                var usersPasswordValue = request.Users.UserPassword;
                var usersNameValue = request.Users.UserName;
                var usersPhoneValue = request.Users.UserPhone;
                var usersAddressValue = request.Users.UserAddress;
                //var usersDepartamentValue = request.Users.ProviderDepartmentId;


                 await _keycloakMsService.CreateUserAsync(usersEmailValue!, usersPasswordValue);
                 var Id = await _keycloakMsService.GetUserByUserName(usersEmailValue);
                // await _authMsService.AssignClientRoleToUser(userId, request.Users.UsersType.ToString()!);

                var users = new Users(
                    UserId.Create(Id),
                    UserEmail.Create(usersEmailValue ),
                    UserPassword.Create(usersPasswordValue ),
                    UserName.Create(usersNameValue ),
                    UserPhone.Create(usersPhoneValue ),
                    UserAddress.Create(usersAddressValue),
                    Enum.Parse<UsersType>(request.Users.UsersType.ToString()!),
                    Enum.Parse<UserAvailable>(request.Users.UserAvailable.ToString()!)
                );

                request.Users.UserId = Id;
                // Publicamos el mensaje en RabbitMQ
                await _eventBus.PublishMessageAsync(request.Users, "userQueue");

                if (request.Users.UsersType == null)
                {
                    throw new NullAtributeException("UsersType can't be null");
                }


                await _usersRepository.AddAsync(users);

                return users.UserId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}