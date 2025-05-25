using UserMs.Application.Commands.User;
using UserMs.Application.Validators;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Core.Service.Keycloak;
using AuthMs.Common.Exceptions;
using FluentValidation;


namespace UserMs.Application.Handlers.User.Commands
{
    public class CreateUsersCommandHandler : IRequestHandler<CreateUsersCommand, UserId>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IEventBus<CreateUsersDto> _eventBus;

        public CreateUsersCommandHandler(IUserRepositoryMongo usersRepositoryMongo,IUserRepository usersRepository, IKeycloakService keycloakMsService, IEventBus<CreateUsersDto> eventBus)
        {
            _usersRepository = usersRepository;
            _keycloakMsService = keycloakMsService;
            _eventBus = eventBus;
            _usersRepositoryMongo = usersRepositoryMongo;
        }



        public async Task<UserId> Handle(CreateUsersCommand request,CancellationToken cancellationToken)
        {
            try
            {


                var validator = new CreateUsersValidator();
                var validationResult = await validator.ValidateAsync(request.Users, cancellationToken);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);
                //var userId = request.Users.UserId;
                var usersEmailValue = request.Users.UserEmail;
                var usersPasswordValue = request.Users.UserPassword;
                var usersNameValue = request.Users.UserName;
                var usersLastNameValue = request.Users.UserLastName;
                var usersPhoneValue = request.Users.UserPhone;
                var usersAddressValue = request.Users.UserAddress;
                var usersDeleteValue = request.Users.UserDelete;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usersPasswordValue);
                //var usersDepartamentValue = request.Users.ProviderDepartmentId;


                await _keycloakMsService.CreateUserAsync(usersEmailValue!, usersPasswordValue, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);
                 var Id = await _keycloakMsService.GetUserByUserName(usersEmailValue);
                 await _keycloakMsService.AssignClientRoleToUser(Id,request.Users.UsersType.ToString()!);

                var users = new Users(
                    Id,
                    //UserId.Create(Id),
                    UserEmail.Create(usersEmailValue ),
                    UserPassword.Create(hashedPassword),
                    UserName.Create(usersNameValue ),
                    UserPhone.Create(usersPhoneValue ),
                    UserAddress.Create(usersAddressValue),
                    UserLastName.Create(request.Users.UserLastName),
                    Enum.Parse<UsersType>(request.Users.UsersType.ToString()!),
                    Enum.Parse<UserAvailable>(request.Users.UserAvailable.ToString()!),
                    UserDelete.Create(usersDeleteValue)
                );



                request.Users.UserId = Id;
               var existingUser = await _usersRepositoryMongo.GetUsersByEmail(users.UserEmail.Value);
               if (existingUser != null)
                {
                   throw new UserExistException("Este Correo ya se encuentra registrado en el sistema.");
                }
                // Publicamos el mensaje en RabbitMQ


                await _usersRepository.AddAsync(users);

                await _eventBus.PublishMessageAsync(request.Users, "userQueue", "USER_CREATED");

                if (request.Users.UsersType == null)
                {
                    throw new NullAtributeException("UsersType can't be null");
                }


                return users.UserId;
            }
            catch (ValidationException ex)
            {

                throw;
            }
            catch (UserExistException ex)
            {

                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}