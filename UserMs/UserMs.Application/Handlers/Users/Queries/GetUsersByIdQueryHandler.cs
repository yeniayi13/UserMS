using UserMs.Application.Queries.User;

using MediatR;
using UserMs.Infrastructure.Exceptions;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;

namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersByIdQueryHandler : IRequestHandler<GetUsersByIdQuery, GetUsersDto>
    {
        private readonly IUserRepositoryMongo _usersRepository;
        private readonly IMapper _mapper;

        public GetUsersByIdQueryHandler(IUserRepositoryMongo usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper;
        }

        public async Task<GetUsersDto> Handle(GetUsersByIdQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersRepository.GetUsersById(request.Id);
            //if (users == null || users.UserDelete.Value)
            // throw new UserNotFoundException("User not found.");

            var userDto = _mapper.Map<GetUsersDto>(users);
            return userDto;
        }
        
    }
}