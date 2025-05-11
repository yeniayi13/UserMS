using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using UserMs.Core.Database;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Application.Dtos.Users.Response;
using MongoDB.Bson;
using AutoMapper;

namespace UserMs.Infrastructure.Repositories
{
    public class UsersRepository : IUserRepository
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMongoCollection<Users> _collection;
        private readonly IMapper _mapper;
        public UsersRepository(IUserDbContext dbContext, IUserDbContextMongo context, IMapper mapper)
        {
            _dbContext = dbContext;
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _collection = context.Database.GetCollection<Users>("Users");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

        public async Task<List<Users>> GetUsersAsync()
        {
            var projection = Builders<Users>.Projection.Exclude("_id"); // Excluir _id

            Console.WriteLine("Consulta en proceso...");

            var usersDto = await _collection
                .Find(Builders<Users>.Filter.Empty)
                .Project<GetUsersDto>(projection) //  Convierte los datos al DTO
                .ToListAsync()
                .ConfigureAwait(false);

            if (usersDto == null || usersDto.Count == 0)
            {
                Console.WriteLine("No se encontraron categorías.");
                return new List<Users>(); // Retorna una lista vacía en lugar de `null` para evitar errores
            }

            var usersEntity = _mapper.Map<List<Users>>(usersDto);

            return usersEntity;

        }

        public async Task<Users?> GetUsersById(Guid userId)
        {
            Console.WriteLine($"Buscando usuario con ID: {userId}");

            // Crear el filtro para buscar por UserId
            var filter = Builders<Users>.Filter.Eq("UserId", userId);

            // Excluir el campo _id de la consulta
            var projection = Builders<Users>.Projection
                            .Exclude("_id");
                            
            // Ejecutar la consulta en MongoDB y proyectar al DTO
            var userDto = await _collection
                .Find(filter)
                .Project<GetUsersDto>(projection)  // Convertir el resultado al DTO
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
            Console.WriteLine(userDto != null ? $"Usuario encontrado: {userDto}" : "Usuario no encontrado.");
            var usersEntity = _mapper.Map<Users>(userDto);


            return usersEntity;
        }

        public async Task<Users?> GetUsersByEmail(UserEmail userEmail)
        {
            return await _collection.Find(user => user.UserEmail == userEmail).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Users users)
        {
            await _dbContext.Users.AddAsync(users);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<Users?> UpdateUsersAsync(UserId userId, Users users)
        {
            _dbContext.Users.Update(users);
            await _dbContext.SaveEfContextChanges("");
            return users;
        }

        public async Task<Users?> DeleteUsersAsync(UserId userId)
        {
            var existingUsers = await _dbContext.Users.FindAsync(userId);
            await _dbContext.SaveEfContextChanges("");
            return existingUsers;
        }
    }
}