using UserMs.Application.Queries.User;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Core.Repositories;

using MediatR;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersByIdQueryHandler : IRequestHandler<GetUsersByIdQuery, GetUsersDto>
    {
        private readonly IUserRepository _usersRepository;

        public GetUsersByIdQueryHandler(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<GetUsersDto> Handle(GetUsersByIdQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersRepository.GetUsersById(request.Id);
            //if (users == null || users.UserDelete.Value)
            // throw new UserNotFoundException("User not found.");


            return users;
        }
        
    }
}