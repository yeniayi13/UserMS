using UserMs.Application.Commands.User;
using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.Interface;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Core.RabbitMQ;
using UserMs.Core;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Application.Dtos.Users.Response;
using AutoMapper;

namespace UserMs.Application.Handlers.User.Commands
{
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand, UserId>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IKeycloakRepository _keycloakRepository;
        private readonly IEventBus<GetUsersDto> _eventBus;
        private readonly IMapper _mapper;
        public DeleteUsersCommandHandler(IUserRepository usersRepository, IKeycloakRepository keycloakRepository, IEventBus<GetUsersDto> eventBus, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _keycloakRepository = keycloakRepository;
            _eventBus = eventBus;
            _mapper = mapper;
        }

        public async Task<UserId> Handle(DeleteUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _usersRepository.GetUsersById(request.UserId);
                // var deleted = await _keycloakRepository.DeleteUserAsync(users!.UserId);
                /* var userEntity = new Users(
                    UserId.Create(users.UserId),
                    //UserId.Create(Id),
                    UserEmail.Create(users.UserEmail.Value),
                    UserPassword.Create(users.UserPassword.Value),
                    UserName.Create(users.UserName.Value),
                    UserPhone.Create(users.UserPhone.Value),
                    UserAddress.Create(users.UserAddress.Value),
                    UserLastName.Create(users.UserLastName.Value),
                    Enum.Parse<UsersType>(users.UsersType.ToString()),
                    Enum.Parse<UserAvailable>(users.UserAvailable.ToString()),
                    UserDelete.Create(users.UserDelete.Value)
                );*/
                users.SetUserDelete(UserDelete.Create(true));
                await _usersRepository.DeleteUsersAsync(request.UserId);
                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBus.PublishMessageAsync(usersDto, "userQueue", "USER_DELETED");
                return request.UserId;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}