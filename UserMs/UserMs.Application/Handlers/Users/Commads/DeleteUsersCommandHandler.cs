using UserMs.Application.Commands.User;
using UserMs.Domain.Entities;
using MediatR;
using UserMs.Core.RabbitMQ;
using UserMs.Core;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Application.Handlers.User.Commands
{
    [ExcludeFromCodeCoverage]
    public class DeleteUsersCommandHandler : IRequestHandler<DeleteUsersCommand, UserId>
    {
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IKeycloakService _keycloakRepository;
        private readonly IEventBus<GetUsersDto> _eventBus;
        private readonly IMapper _mapper;
        public DeleteUsersCommandHandler(IUserRepositoryMongo usersRepositoryMongo,IUserRepository usersRepository, IKeycloakService keycloakRepository, IEventBus<GetUsersDto> eventBus, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _keycloakRepository = keycloakRepository;
            _eventBus = eventBus;
            _mapper = mapper;
            _usersRepositoryMongo = usersRepositoryMongo;
        }

        public async Task<UserId> Handle(DeleteUsersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _usersRepositoryMongo.GetUsersById(request.UserId);
                var deleted = await _keycloakRepository.DeleteUserAsync(users!.UserId);
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