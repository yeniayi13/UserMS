using UserMs.Application.Queries.User;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Core.Repositories;
using MediatR;

namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<GetUsersDto>>
    {
        private readonly IUserRepository _usersRepository;
       
        public GetUsersQueryHandler(IUserRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<List<GetUsersDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersRepository.GetUsersAsync();
            //var activeUsers = users.Where(u => !u.UserDelete.Value).ToList();

            return users;
        }
    }
}