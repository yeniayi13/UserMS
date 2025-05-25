using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthMs.Common.Exceptions;
using FluentValidation;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using UserMs.Application.Validators;
using BCrypt.Net;


namespace UserMs.Application.Handlers.Auctioneer.Command
{
    public class CreateAuctioneerCommandHandler : IRequestHandler<CreateAuctioneerCommand, UserId>
    {
      
            private readonly IAuctioneerRepository _auctioneerRepository;
            private readonly IAuctioneerRepositoryMongo _auctioneerRepositoryMongo;
            private readonly IEventBus<GetAuctioneerDto> _eventBus;
            private readonly IMapper _mapper;
            private readonly IKeycloakService _keycloakMsService;
            private readonly IEventBus<GetUsersDto> _eventBusUser;
            private readonly IUserRepository _usersRepository;
            private readonly IUserRoleRepository _userRoleRepository;
            private readonly IUserRepositoryMongo _usersRepositoryMongo;
            private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
            private readonly IRolesRepository _roleRepository;
            private readonly IActivityHistoryRepository _activityHistoryRepository;
            private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
            private readonly IEventBus<GetUserRoleDto> _eventBusUserRol;

            public CreateAuctioneerCommandHandler(
                IAuctioneerRepository auctioneerRepository,
                IAuctioneerRepositoryMongo auctioneerRepositoryMongo,
                IEventBus<GetAuctioneerDto> eventBus,
                IMapper mapper,
                IKeycloakService keycloakMsService,
                IEventBus<GetUsersDto> eventBusUser,
                IUserRepository usersRepository,
                IUserRoleRepository userRoleRepository,
                IUserRepositoryMongo usersRepositoryMongo,
                IUserRoleRepositoryMongo userRoleRepositoryMongo,
                IRolesRepository roleRepository,
                IActivityHistoryRepository activityHistoryRepository,
                IEventBus<GetActivityHistoryDto> eventBusActivity,
                IEventBus<GetUserRoleDto> eventBusUserRol)
            {
                _auctioneerRepository = auctioneerRepository;
                _auctioneerRepositoryMongo = auctioneerRepositoryMongo;
                _eventBus = eventBus;
                _mapper = mapper;
                _keycloakMsService = keycloakMsService;
                _eventBusUser = eventBusUser;
                _usersRepository = usersRepository;
                _userRoleRepository = userRoleRepository;
                _usersRepositoryMongo = usersRepositoryMongo;
                _userRoleRepositoryMongo = userRoleRepositoryMongo;
                _roleRepository = roleRepository;
                _activityHistoryRepository = activityHistoryRepository;
                _eventBusActivity = eventBusActivity;
                _eventBusUserRol = eventBusUserRol;
            }
        

        public async Task<UserId> Handle(CreateAuctioneerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validación de datos de entrada
                var validator = new CreateAuctioneersValidator();
                var validationResult = await validator.ValidateAsync(request.Auctioneer, cancellationToken);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var userEmailValue = request.Auctioneer.UserEmail;
                var userPasswordValue = request.Auctioneer.UserPassword;
                var usersNameValue = request.Auctioneer.UserName;
                var usersLastNameValue = request.Auctioneer.UserLastName;
                var usersPhoneValue = request.Auctioneer.UserPhone;
                var usersAddressValue = request.Auctioneer.UserAddress;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userPasswordValue);

                // Validar si el usuario ya existe
                var userExists =
                    await _auctioneerRepositoryMongo.GetAuctioneerByEmailAsync(UserEmail.Create(userEmailValue));
                if (userExists != null)
                    throw new UserExistException("El usuario ya existe");

                // Creación del usuario en Keycloak
                await _keycloakMsService.CreateUserAsync(userEmailValue!, userPasswordValue, usersNameValue,
                    usersLastNameValue, usersPhoneValue, usersAddressValue);
                var Id = await _keycloakMsService.GetUserByUserName(userEmailValue);
                await _keycloakMsService.AssignClientRoleToUser(Id, "Subastador");

                var auctioneer = new Auctioneers(
                    UserId.Create(Id),
                    UserEmail.Create(userEmailValue ?? string.Empty),
                    UserPassword.Create(userPasswordValue ?? string.Empty),
                    UserName.Create(usersNameValue ?? string.Empty),
                    UserPhone.Create(usersPhoneValue ?? string.Empty),
                    UserAddress.Create(usersAddressValue ?? string.Empty),
                    UserLastName.Create(usersLastNameValue ?? string.Empty),
                    AuctioneerDni.Create(request.Auctioneer.AuctioneerDni),
                    AuctioneerBirthday.Create(request.Auctioneer.AuctioneerBirthday)
                );

                var users = new Users(
                    Id,
                    UserEmail.Create(userEmailValue),
                    UserPassword.Create(hashedPassword),
                    UserName.Create(usersNameValue),
                    UserPhone.Create(usersPhoneValue),
                    UserAddress.Create(usersAddressValue),
                    UserLastName.Create(usersLastNameValue),
                    Enum.Parse<UsersType>("Subastador"),
                    Enum.Parse<UserAvailable>("Activo"),
                    UserDelete.Create(false)
                );

                var role = await _roleRepository.GetRolesByNameQuery("Subastador");
                if (role == null)
                    throw new RoleNotFoundException("Role not found");

                var exist = await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(role.RoleName.Value, userEmailValue);
                if (exist != null)
                    throw new UserRoleExistException("Este usuario ya tiene este rol.");

                var userRole = new UserRoles(
                    UserRoleId.Create(Guid.NewGuid()),
                    UserId.Create(Id),
                    RoleId.Create(role.RoleId)
                );

                // Registro en repositorios
                await _usersRepository.AddAsync(users);
                await _auctioneerRepository.AddAsync(auctioneer);
                await _userRoleRepository.AddAsync(userRole);

                // Publicar eventos en el bus de mensajes
                await _eventBus.PublishMessageAsync(_mapper.Map<GetAuctioneerDto>(auctioneer), "auctioneerQueue",
                    "AUCTIONEER_CREATED");
                await _eventBusUser.PublishMessageAsync(_mapper.Map<GetUsersDto>(users), "userQueue", "USER_CREATED");
                await _eventBusUserRol.PublishMessageAsync(_mapper.Map<GetUserRoleDto>(userRole), "userRoleQueue",
                    "USER_ROLE_CREATED");

                // Registrar actividad
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    Id,
                    "Creación de Subastador",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                await _eventBusActivity.PublishMessageAsync(_mapper.Map<GetActivityHistoryDto>(activity),
                    "activityHistoryQueue", "ACTIVITY_CREATED");

                return auctioneer.UserId;
            }
            catch (ValidationException ex)
            {

                throw;
            }
            catch (UserExistException ex)
            {

                throw;
            }
            catch (RoleNotFoundException ex)
            {
                throw;
            }
            catch (UserRoleExistException ex)
            {

                throw;
            }
            catch (Exception ex)
            {

                throw new ApplicationException("Ocurrió un error al crear el subastador.", ex);
            }
        }
    }
}
