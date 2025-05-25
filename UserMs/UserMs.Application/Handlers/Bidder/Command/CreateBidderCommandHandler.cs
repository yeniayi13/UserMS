using AuthMs.Common.Exceptions;
using AutoMapper;
using FluentValidation;
using MediatR;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Validators;
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
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Bidder.Command
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
                // Validación de datos de entrada
                var validator = new CreateBidderValidator();
                var validationResult = await validator.ValidateAsync(request.Bidder, cancellationToken);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var userEmailValue = request.Bidder.UserEmail;
                var userPasswordValue = request.Bidder.UserPassword;
                var usersNameValue = request.Bidder.UserName;
                var usersLastNameValue = request.Bidder.UserLastName;
                var usersPhoneValue = request.Bidder.UserPhone;
                var usersAddressValue = request.Bidder.UserAddress;
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userPasswordValue);

                var userExists = await _bidderRepositoryMongo.GetBidderByEmailAsync(UserEmail.Create(userEmailValue));
                if (userExists != null)
                    throw new UserExistException("El usuario ya existe");

                await _keycloakMsService.CreateUserAsync(userEmailValue!, userPasswordValue, usersNameValue,
                    usersLastNameValue, usersPhoneValue, usersAddressValue);
                var Id = await _keycloakMsService.GetUserByUserName(userEmailValue);
                await _keycloakMsService.AssignClientRoleToUser(Id, "Postor");

                var bidderId = BidderId.Create();
                var bidder = new Bidders(
                    UserId.Create(Id),
                    UserEmail.Create(userEmailValue ?? string.Empty),
                    UserPassword.Create(hashedPassword ?? string.Empty),
                    UserName.Create(usersNameValue ?? string.Empty),
                    UserPhone.Create(usersPhoneValue ?? string.Empty),
                    UserAddress.Create(usersAddressValue ?? string.Empty),
                    UserLastName.Create(usersLastNameValue ?? string.Empty),
                    BidderDni.Create(request.Bidder.BidderDni),
                    BidderBirthday.Create(request.Bidder.BidderBirthday)
                );

                var users = new Users(
                    Id,
                    UserEmail.Create(userEmailValue),
                    UserPassword.Create(hashedPassword),
                    UserName.Create(usersNameValue),
                    UserPhone.Create(usersPhoneValue),
                    UserAddress.Create(usersAddressValue),
                    UserLastName.Create(usersLastNameValue),
                    Enum.Parse<UsersType>("Postor"),
                    Enum.Parse<UserAvailable>("Activo"),
                    UserDelete.Create(false)
                );

                var role = await _roleRepository.GetRolesByNameQuery("Postor");
                if (role == null)
                    throw new RoleNotFoundException("Role not found");

                var exist = await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(role.RoleName.Value, userEmailValue);
                if (exist != null)
                    throw new UserRoleExistException("Este usuario posee este rol ");

                var userRole = new UserRoles(
                    UserRoleId.Create(Guid.NewGuid()),
                    UserId.Create(Id),
                    RoleId.Create(role.RoleId)
                );

                await _bidderRepository.AddAsync(bidder);
                await _usersRepository.AddAsync(users);
                await _userRoleRepository.AddAsync(userRole);

                var bidderDto = _mapper.Map<GetBidderDto>(bidder);
                await _eventBus.PublishMessageAsync(bidderDto, "bidderQueue", "BIDDER_CREATED");

                var userDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_CREATED");

                var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);
                await _eventBusUserRol.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_CREATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    Id,
                    "Creación de Postor",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return bidder.UserId;
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

                throw new ApplicationException("Ocurrió un error inesperado al crear el postor.", ex);
            }
        }
}
}
