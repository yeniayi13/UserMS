using UserMs.Application.Queries.User;
using MediatR;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Infrastructure.Exceptions;


namespace UserMs.Application.Handlers.User.Queries
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<GetUsersDto>>
    {
        private readonly IUserRepositoryMongo _usersRepository;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(IUserRepositoryMongo usersRepository, IMapper mapper)
        {
            _usersRepository = usersRepository;
            _mapper = mapper; // Inyectar el Mapper
        }

        public async Task<List<GetUsersDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {

            try
            {
                    var users = await _usersRepository.GetUsersAsync();
                //var activeUsers = users.Where(u => !u.UserDelete.Value).ToList();
                if (users == null || users.Count == 0)
                {
                    throw new UserNotFoundException("User not found."); //  Retornar lista vacía en lugar de `null`
                }

                var userDto = _mapper.Map < List<GetUsersDto>>(users);
                return userDto;
            }
            catch (Exception ex)
            {
                throw; //  Retornar lista vacía en caso de error
            }
        }
    }
}