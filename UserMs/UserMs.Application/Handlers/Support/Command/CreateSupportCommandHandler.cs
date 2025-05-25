using AuthMs.Common.Exceptions;
using AutoMapper;
using MediatR;
using UserMs.Application.Commands.Support;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Support.Command
{
    public class CreateSupportCommandHandler : IRequestHandler<CreateSupportCommand, UserId>
    {
        private readonly ISupportRepository _supportRepository;
        private readonly ISupportRepositoryMongo _supportRepositoryMongo;
        private readonly IEventBus<GetSupportDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
        private readonly IRolesRepository _roleRepository;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private IEventBus<GetUserRoleDto> _eventBusUserRol;

        public CreateSupportCommandHandler(
            IEventBus<GetUserRoleDto> eventBusUserRol,
            IActivityHistoryRepository activityHistoryRepository,
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IUserRoleRepository userRoleRepository,
            IUserRoleRepositoryMongo userRoleRepositoryMongo,
            IRolesRepository roleRepository,
            ISupportRepository supportRepository,
            ISupportRepositoryMongo supportRepositoryMongo,
            IUserRepository usersRepository,
            IEventBus<GetSupportDto> eventBus,
            IMapper mapper,
            IKeycloakService keycloakMsService,
            IEventBus<GetUsersDto> eventBusUser)
        {
            _supportRepository = supportRepository;
            _supportRepositoryMongo = supportRepositoryMongo;
            _eventBus = eventBus;
            _eventBusUser = eventBusUser;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _usersRepository = usersRepository;
            _userRoleRepository = userRoleRepository;
            _userRoleRepositoryMongo = userRoleRepositoryMongo;
            _roleRepository = roleRepository;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
            _eventBusUserRol = eventBusUserRol;
        }


        public async Task<UserId> Handle(CreateSupportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userEmailValue = request.Support.UserEmail;
                var userPasswordValue = request.Support.UserPassword;
                var usersNameValue = request.Support.UserName;
                var usersLastNameValue = request.Support.UserLastName;
                var usersPhoneValue = request.Support.UserPhone;
                var usersAddressValue = request.Support.UserAddress;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userPasswordValue);
                var userExists = await _supportRepositoryMongo.GetSupportByEmailAsync(UserEmail.Create(userEmailValue));
                if (userExists != null)
                {
                    throw new UserExistException("El usuario ya existe");
                }

                await _keycloakMsService.CreateUserAsync(userEmailValue!, userPasswordValue, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);
                var Id = await _keycloakMsService.GetUserByUserName(userEmailValue);
                await _keycloakMsService.AssignClientRoleToUser(Id, "Soporte");

                var supportId = SupportId.Create();
                var support = new Supports(
                    UserId.Create(Id),
                    UserEmail.Create(userEmailValue ?? string.Empty),
                    UserPassword.Create(hashedPassword ?? string.Empty),
                    UserName.Create(usersNameValue ?? string.Empty),
                    UserPhone.Create(usersPhoneValue ?? string.Empty),
                    UserAddress.Create(usersAddressValue ?? string.Empty),
                    UserLastName.Create(usersLastNameValue ?? string.Empty),
                    SupportDni.Create(request.Support.SupportDni),
                    Enum.Parse<SupportSpecialization>(request.Support.SupportSpecialization.ToString()!)
                );

                var users = new Users(
                    Id,
                    UserEmail.Create(userEmailValue),
                    UserPassword.Create(hashedPassword),
                    UserName.Create(usersNameValue),
                    UserPhone.Create(usersPhoneValue),
                    UserAddress.Create(usersAddressValue),
                    UserLastName.Create(usersLastNameValue),
                    Enum.Parse<UsersType>("Soporte"),
                    Enum.Parse<UserAvailable>("Activo"),
                    UserDelete.Create(false)
                );

                var role = await _roleRepository.GetRolesByNameQuery("Soporte");
                if (role == null)
                {
                    throw new RoleNotFoundException("Role not found");
                }

                var exist = await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(role.RoleName.Value, userEmailValue);
                if (exist != null)
                {
                    throw new UserRoleExistException("Este usuario posee este rol");
                }

                var userRole = new UserRoles(
                    UserRoleId.Create(Guid.NewGuid()),
                    UserId.Create(Id),
                    RoleId.Create(role.RoleId)
                );

                await _usersRepository.AddAsync(users);
                await _supportRepository.AddAsync(support);
                await _userRoleRepository.AddAsync(userRole);

                var usersDto = _mapper.Map<GetSupportDto>(support);
                await _eventBus.PublishMessageAsync(usersDto, "supportQueue", "SUPPORT_CREATED");

                var userDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

                var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);
                await _eventBusUserRol.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    support.UserId,
                    "Creó un trabajador de soporte",
                    DateTime.UtcNow
                );
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
                await _activityHistoryRepository.AddAsync(activity);

                return support.UserId;
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
               
                throw new ApplicationException("Ocurrió un error inesperado al crear el soporte.", ex);
            }
        }
    }
}
