using AuthMs.Common.Exceptions;
using AutoMapper;
using FluentValidation;
using MediatR;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;

namespace Handlers.Bidder.Command
{

    public class CreateBidderCommandHandler : IRequestHandler<CreateBidderCommand, UserId>
    {
        private readonly IBidderRepository _bidderRepository;
        private readonly IBidderRepositoryMongo _bidderRepositoryMongo;
        private readonly IEventBus<GetBidderDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IUserRepository _usersRepository;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
        private readonly IRolesRepository _roleRepository;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private IEventBus<GetUserRoleDto> _eventBusUserRol;

        public CreateBidderCommandHandler(
            IEventBus<GetUserRoleDto> eventBusUserRol,
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IUserRoleRepository userRoleRepository,
            IRolesRepository roleRepository,
            IUserRepository usersRepository,
            IUserRoleRepositoryMongo userRoleRepositoryMongo,
            IBidderRepository bidderRepository,
            IBidderRepositoryMongo bidderRepositoryMongo,
            IEventBus<GetBidderDto> eventBus,
            IMapper mapper,
            IKeycloakService keycloakMsService,
            IEventBus<GetUsersDto> eventBusUser)
        {
            _bidderRepository = bidderRepository;
            _bidderRepositoryMongo = bidderRepositoryMongo;
            _eventBus = eventBus;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _usersRepository = usersRepository;
            _userRoleRepository = userRoleRepository;
            _userRoleRepositoryMongo = userRoleRepositoryMongo;
            _roleRepository = roleRepository;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
            _eventBusUserRol = eventBusUserRol;
            _eventBusUser = eventBusUser;
        }


        public async Task<UserId> Handle(CreateBidderCommand request, CancellationToken cancellationToken)
        {

            try

            {
                var userEmailValue = request.Bidder.UserEmail;
                var userPasswordValue = request.Bidder.UserPassword;
                var usersNameValue = request.Bidder.UserName;
                var usersLastNameValue = request.Bidder.UserLastName;
                var usersPhoneValue = request.Bidder.UserPhone;
                var usersAddressValue = request.Bidder.UserAddress;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userPasswordValue);

                // ✅ Validación de datos de entrada
                await ValidateBidderRequest(request, cancellationToken);

                // ✅ Creación y verificación del usuario
                var Id = await CreateUserInKeycloak(userEmailValue, hashedPassword, usersNameValue, usersLastNameValue, usersPhoneValue, usersAddressValue);

                // ✅ Creación de entidades
                var bidder = CreateBidderEntity(request.Bidder, Id);
                var users = CreateUserEntity(request.Bidder, Id);
                var userRole = await CreateUserRoleEntity(Id, request.Bidder.UserEmail);

                // ✅ Almacenamiento en repositorios
                await SaveEntities(bidder, users, userRole);

                // ✅ Publicación de eventos
                await PublishEvents(bidder, users, userRole, Id);

                return bidder.UserId;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }

        private async Task ValidateBidderRequest(CreateBidderCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateBidderValidator();
            var validationResult = await validator.ValidateAsync(request.Bidder, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var userExists = await _bidderRepositoryMongo.GetBidderByEmailAsync(UserEmail.Create(request.Bidder.UserEmail));
            if (userExists != null)
                throw new UserExistException("El usuario ya existe");
        }

        private async Task<Guid> CreateUserInKeycloak(string userEmail, string userPassword, string userName, string userLastName, string userPhone, string userAddress)
        {
            await _keycloakMsService.CreateUserAsync(userEmail, userPassword, userName, userLastName, userPhone, userAddress);

            var userId = await _keycloakMsService.GetUserByUserName(userEmail);

            if (userId == Guid.Empty)
                throw new Exception("No se pudo obtener el ID del usuario desde Keycloak.");

            await _keycloakMsService.AssignClientRoleToUser(userId, "Postor");

            return userId;
        }

        private Bidders CreateBidderEntity(CreateBidderDto bidder, Guid userId)
        {
            return new Bidders(
                UserId.Create(userId),
                UserEmail.Create(bidder.UserEmail ?? string.Empty),
                UserPassword.Create(BCrypt.Net.BCrypt.HashPassword(bidder.UserPassword) ?? string.Empty),
                UserName.Create(bidder.UserName ?? string.Empty),
                UserPhone.Create(bidder.UserPhone ?? string.Empty),
                UserAddress.Create(bidder.UserAddress ?? string.Empty),
                UserLastName.Create(bidder.UserLastName ?? string.Empty),
                BidderDni.Create(bidder.BidderDni),
                BidderBirthday.Create(bidder.BidderBirthday)
            );
        }

        private Users CreateUserEntity(CreateBidderDto bidder, Guid userId)
        {
            return new Users(
                userId,
                UserEmail.Create(bidder.UserEmail),
                UserPassword.Create(BCrypt.Net.BCrypt.HashPassword(bidder.UserPassword)),
                UserName.Create(bidder.UserName),
                UserPhone.Create(bidder.UserPhone),
                UserAddress.Create(bidder.UserAddress),
                UserLastName.Create(bidder.UserLastName),
                Enum.Parse<UsersType>("Postor"),
                Enum.Parse<UserAvailable>("Activo"),
                UserDelete.Create(false)
            );
        }

        private async Task<UserRoles> CreateUserRoleEntity(Guid userId, string userEmail)
        {
            var role = await _roleRepository.GetRolesByNameQuery("Postor");
            if (role == null)
                throw new RoleNotFoundException("Role not found");

            var exist = await _userRoleRepositoryMongo.GetRoleByRoleNameAndByUserEmail(role.RoleName.Value, userEmail);
            if (exist != null)
                throw new UserRoleExistException("Este usuario posee este rol");

            return new UserRoles(
                UserRoleId.Create(Guid.NewGuid()),
                UserId.Create(userId),
                RoleId.Create(role.RoleId)
            );
        }

        private async Task SaveEntities(Bidders bidder, Users users, UserRoles userRole)
        {
            await _bidderRepository.AddAsync(bidder);
            await _usersRepository.AddAsync(users);
            await _userRoleRepository.AddAsync(userRole);
        }

        private async Task PublishEvents(Bidders bidder,Users users, UserRoles userRole, Guid userId)
        {
            var bidderDto = _mapper.Map<GetBidderDto>(bidder);
            await _eventBus.PublishMessageAsync(bidderDto, "bidderQueue", "BIDDER_CREATED");

            var userDto = _mapper.Map<GetUsersDto>(users);
            await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

            var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);
            userRoleDto.UserEmail = users.UserEmail.Value;
            userRoleDto.RoleName = await _roleRepository.GetRolesByNameQuery("Postor").ContinueWith(t => t.Result.RoleName.Value);
            await _eventBusUserRol.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");

            var activity = new ActivityHistory(
                Guid.NewGuid(),
                userId,
                "Creación de Postor",
                DateTime.UtcNow
            );
            await _activityHistoryRepository.AddAsync(activity);

            var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }
    }


}
