using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Database;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Infrastructure.Repositories.User_Roles
{
    public class UserRolesRepository : IUserRoleRepository
    {

        private readonly IUserDbContext _dbContext;
        private readonly IMapper _mapper;
        public UserRolesRepository(IUserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
           
        }
        public async Task AddAsync(UserRoles userRoles)
        {
            await _dbContext.UserRoles.AddAsync(userRoles);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<UserRoles?> DeleteUsersRoleAsync(UserRoleId id)
        {
            var existingUsers = await _dbContext.UserRoles.FindAsync(id);

            if (existingUsers != null)
            {
                _dbContext.UserRoles.Remove(existingUsers);
                await _dbContext.SaveEfContextChanges("");
            }

            return existingUsers;
        }


        public async Task<UserRoles?> UpdateUsersRoleAsync(UserId userId, UserRoles userRole)
        {
            _dbContext.UserRoles.Update(userRole);
            await _dbContext.SaveEfContextChanges("");
            return userRole;
        }
    }
}
