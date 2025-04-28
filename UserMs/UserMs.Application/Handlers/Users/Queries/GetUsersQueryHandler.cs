using UserMs.Application.Queries.User;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Core.Repositories;
using MediatR;

namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<GetUsersDto>>
    {
        private readonly IUsersRepository _usersRepository;
       
        public GetUsersQueryHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<List<GetUsersDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersRepository.GetUsersAsync();
            var activeUsers = users.Where(u => !u.UserDelete.Value).ToList();

            return activeUsers.Select(users => new GetUsersDto
            {
                UserId = users.UserId.Value,
                UserEmail = users.UserEmail.Value,
                UserPassword = users.UserPassword.Value,
                UsersType = users.GetUsersTypeString(),
            }).ToList();
        }
    }
}