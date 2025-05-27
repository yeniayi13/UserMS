using AuthMs.Common.Exceptions;
using AutoMapper;
using FluentValidation;
using MediatR;
using UserMs.Application.Commands.Support;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos.Users.Request.Support;
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

                // ✅ Validación de datos de entrada
                await ValidateSupportRequest(request, cancellationToken);

                // ✅ Creación y verificación del usuario
                var Id = await CreateUserInKeycloak(userEmailValue, hashedPassword, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);

                // ✅ Creación de entidades
                var support = CreateSupportEntity(request.Support, Id);
                var users = CreateUserEntity(request.Support, Id);
                var userRole = await CreateUserRoleEntity(Id, request.Support.UserEmail);

                // ✅ Almacenamiento en repositorios
                await SaveEntities(users, support, userRole);

                // ✅ Publicación de eventos
                await PublishEvents(support, users, userRole, Id);

                return support.UserId;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }
        private async Task ValidateSupportRequest(CreateSupportCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateSupportValidator();
            var validationResult = await validator.ValidateAsync(request.Support, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var userExists = await _supportRepositoryMongo.GetSupportByEmailAsync(UserEmail.Create(request.Support.UserEmail));
            if (userExists != null)
                throw new UserExistException("El usuario ya existe");
        }

        private async Task<Guid> CreateUserInKeycloak(string userEmail, string userPassword, string userName, string userLastName, string userPhone, string userAddress)
        {
            await _keycloakMsService.CreateUserAsync(userEmail, userPassword, userName, userLastName, userPhone, userAddress);

            var userId = await _keycloakMsService.GetUserByUserName(userEmail);

            if (userId == Guid.Empty)
                throw new Exception("No se pudo obtener el ID del usuario desde Keycloak.");

            await _keycloakMsService.AssignClientRoleToUser(userId, "Soporte");

            return userId;
        }

        private Supports CreateSupportEntity(CreateSupportDto supportDto, Guid userId)
        {
            return new Supports(
                UserId.Create(userId),
                UserEmail.Create(supportDto.UserEmail ?? string.Empty),
                UserPassword.Create(BCrypt.Net.BCrypt.HashPassword(supportDto.UserPassword) ?? string.Empty),
                UserName.Create(supportDto.UserName ?? string.Empty),
                UserPhone.Create(supportDto.UserPhone ?? string.Empty),
                UserAddress.Create(supportDto.UserAddress ?? string.Empty),
                UserLastName.Create(supportDto.UserLastName ?? string.Empty),
                SupportDni.Create(supportDto.SupportDni),
                Enum.Parse<SupportSpecialization>(supportDto.SupportSpecialization.ToString()!)
            );
        }
        private Users CreateUserEntity(CreateSupportDto supportDto, Guid userId)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(supportDto.UserPassword);

            return new Users(
                userId,
                UserEmail.Create(supportDto.UserEmail),
                UserPassword.Create(hashedPassword),
                UserName.Create(supportDto.UserName),
                UserPhone.Create(supportDto.UserPhone),
                UserAddress.Create(supportDto.UserAddress),
                UserLastName.Create(supportDto.UserLastName),
                Enum.Parse<UsersType>("Soporte"),
                Enum.Parse<UserAvailable>("Activo"),
                UserDelete.Create(false)
            );
        }
        private async Task<UserRoles> CreateUserRoleEntity(Guid userId, string userEmail)
        {
            var role = await _roleRepository.GetRolesByNameQuery("Soporte");
            if (role == null)
                throw new RoleNotFoundException("Role not found");

            var exist = await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(role.RoleName.Value, userEmail);
            if (exist != null)
                throw new UserRoleExistException("Este usuario ya tiene este rol.");

            return new UserRoles(
                UserRoleId.Create(Guid.NewGuid()),
                UserId.Create(userId),
                RoleId.Create(role.RoleId)
            );
        }

        private async Task SaveEntities(Users users, Supports support, UserRoles userRole)
        {
            await _usersRepository.AddAsync(users);
            await _supportRepository.AddAsync(support);
            await _userRoleRepository.AddAsync(userRole);
        }

        private async Task PublishEvents(Supports support, Users users, UserRoles userRole, Guid userId)
        {
            var supportDto = _mapper.Map<GetSupportDto>(support);
            await _eventBus.PublishMessageAsync(supportDto, "supportQueue", "SUPPORT_CREATED");

            var userDto = _mapper.Map<GetUsersDto>(users);
            await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

            var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);
            userRoleDto.UserEmail = users.UserEmail.Value;
            userRoleDto.RoleName = await _roleRepository.GetRolesByNameQuery("Soporte").ContinueWith(t => t.Result.RoleName.Value);

            await _eventBusUserRol.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");

            var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                Guid.NewGuid(),
                userId,
                "Creación de Soporte",
                DateTime.UtcNow
            );
            await _activityHistoryRepository.AddAsync(activity);

            var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }

    }
}
