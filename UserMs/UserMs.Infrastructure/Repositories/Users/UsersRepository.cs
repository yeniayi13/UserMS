
using UserMs.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using UserMs.Core.Database;
using MongoDB.Driver;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using UserMs.Domain.Entities.IUser.ValueObjects;
using MongoDB.Bson;
using AutoMapper;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.UserEntity;
//using UserMs.Domain.Entities.UserEntity;


namespace UserMs.Infrastructure.Repositories
{
    public class UsersRepository : IUserRepository
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMapper _mapper;
        public UsersRepository(IUserDbContext dbContext,IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  
        }

        public async Task AddAsync(Domain.Entities.UserEntity.Users users)
        {
            await _dbContext.Users.AddAsync(users);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<Domain.Entities.UserEntity.Users?> UpdateUsersAsync(UserId userId, Domain.Entities.UserEntity.Users users)
        {
            _dbContext.Users.Update(users);
            await _dbContext.SaveEfContextChanges("");
            return users;
        }

        public async Task<Domain.Entities.UserEntity.Users?> DeleteUsersAsync(UserId userId)
        {
            var existingUsers = await _dbContext.Users.FindAsync(userId);
            await _dbContext.SaveEfContextChanges("");
            return existingUsers;
        }

    }
}