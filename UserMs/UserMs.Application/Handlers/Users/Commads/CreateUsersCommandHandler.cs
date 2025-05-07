using UserMs.Application.Commands.User;
using UserMs.Application.Validators;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request;


namespace UserMs.Application.Handlers.User.Commands
{
    public class CreateUsersCommandHandler : IRequestHandler<CreateUsersCommand, UserId>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IKeycloakRepository _keycloakMsService;
        private readonly IEventBus<CreateUsersDto> _eventBus;

        public CreateUsersCommandHandler(IUserRepository usersRepository, IKeycloakRepository keycloakMsService, IEventBus<CreateUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakMsService = keycloakMsService;
            _eventBus = eventBus;
        }



        public async Task<UserId> Handle(CreateUsersCommand request,CancellationToken cancellationToken)
        {
            try
            {
                

                var validator = new CreateUsersValidator();
                await validator.ValidateRequest(request.Users);
                //var userId = request.Users.UserId;
                var usersEmailValue = request.Users.UserEmail;
                var usersPasswordValue = request.Users.UserPassword;
                var usersNameValue = request.Users.UserName;
                var usersLastNameValue = request.Users.UserLastName;
                var usersPhoneValue = request.Users.UserPhone;
                var usersAddressValue = request.Users.UserAddress;
                var usersDeleteValue = request.Users.UserDelete;
                //var usersDepartamentValue = request.Users.ProviderDepartmentId;


                await _keycloakMsService.CreateUserAsync(usersEmailValue!, usersPasswordValue, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);
                 var Id = await _keycloakMsService.GetUserByUserName(usersEmailValue);
                 await _keycloakMsService.AssignClientRoleToUser(Id,request.Users.UsersType.ToString()!);

                var users = new Users(
                    Id,
                    //UserId.Create(Id),
                    UserEmail.Create(usersEmailValue ),
                    UserPassword.Create(usersPasswordValue ),
                    UserName.Create(usersNameValue ),
                    UserPhone.Create(usersPhoneValue ),
                    UserAddress.Create(usersAddressValue),
                    UserLastName.Create(request.Users.UserLastName),
                    Enum.Parse<UsersType>(request.Users.UsersType.ToString()!),
                    Enum.Parse<UserAvailable>(request.Users.UserAvailable.ToString()!),
                    UserDelete.Create(usersDeleteValue)
                );



                request.Users.UserId = Id;
               //var existingUser = await _usersRepository.GetUsersByEmail(users.UserEmail);
              //  if (existingUser != null)
               // {
                //    throw new Exception("Este Correo ya se encuentra registrado en el sistema.");
                //}
                // Publicamos el mensaje en RabbitMQ


                await _usersRepository.AddAsync(users);

                await _eventBus.PublishMessageAsync(request.Users, "userQueue", "USER_CREATED");

                if (request.Users.UsersType == null)
                {
                    throw new NullAtributeException("UsersType can't be null");
                }


                return users.UserId;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}