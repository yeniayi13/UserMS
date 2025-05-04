using UserMs.Core.Repositories;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using UserMs.Core.Database;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UserMs.Infrastructure.Repositories
{
    public class UsersRepository : IUserRepository
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMongoCollection<Users> _collection;

        public UsersRepository(IUserDbContext dbContext, IUserDbContextMongo context)
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

           // _collection = context.Database.GetCollection<Users>("Company");
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

        public async Task<List<Users>> GetUsersAsync()
        {
            return await _collection.Find(Builders<Users>.Filter.Empty).ToListAsync();
        }

        public async Task<Users?> GetUsersById(UserId userId)
        {
            return await _collection.Find(user => user.UserId == userId).FirstOrDefaultAsync();
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
            var existingUsers = await _dbContext.Users.FindAsync(userId);
            await _dbContext.SaveEfContextChanges("");
            return existingUsers;
        }

        public async Task<Users?> DeleteUsersAsync(UserId userId)
        {
            var existingUsers = await _dbContext.Users.FindAsync(userId);
            await _dbContext.SaveEfContextChanges("");
            return existingUsers;
        }
    }
}