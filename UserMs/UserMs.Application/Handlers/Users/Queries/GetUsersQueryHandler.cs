using UserMs.Application.Queries.User;
using MediatR;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;


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
                    Console.WriteLine("No se encontraron los usuarios con el estado solicitado.");
                    return new List<GetUsersDto>(); //  Retornar lista vacía en lugar de `null`
                }

                var userDto = _mapper.Map < List<GetUsersDto>>(users);
                return userDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetUsersDto>(); //  Retornar lista vacía en caso de error
            }
        }
    }
}