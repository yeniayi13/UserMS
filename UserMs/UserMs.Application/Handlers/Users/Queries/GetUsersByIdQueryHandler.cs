using UserMs.Application.Queries.User;
using UserMs.Application.Dtos.Users.Response;
using UserMs.Core.Repositories;

using MediatR;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersByIdQueryHandler : IRequestHandler<GetUsersByIdQuery, GetUsersDto>
    {
        private readonly IUsersRepository _usersRepository;

        public GetUsersByIdQueryHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<GetUsersDto> Handle(GetUsersByIdQuery request, CancellationToken cancellationToken)
        {
            var users = await _usersRepository.GetUsersById(request.Id);

            if (users == null)
               throw new UserNotFoundException("User not found.");

            if (users.UserDelete.Value)
                throw new UserNotFoundException("User not found.");

            return new GetUsersDto
            {
                UserId = users.UserId.Value,
                UserEmail = users.UserEmail.Value,
                UserPassword = users.UserPassword.Value,
                UserName = users.UserName.Value,
                UserPhone = users.UserPhone.Value,
                UserAddress = users.UserAddress.Value,
                UsersType = users.GetUsersTypeString(),
               
            };
        }
    }
}