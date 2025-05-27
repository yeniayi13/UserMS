using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Database;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Infrastructure.Repositories.Roles_Permission
{
    public class RolePermissionRepository : IRolePermissionRepository
    {

        private readonly IUserDbContext _dbContext;
       
        private readonly IMapper _mapper;
        public RolePermissionRepository(IUserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
           
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            
            //?? //throw new ArgumentNullException(nameof(context.Database.GetCollection<StatusProvider>));
        }

        public async Task AddAsync(RolePermissions rolePermissions)
        {
            await _dbContext.RolePermissions.AddAsync(rolePermissions);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<RolePermissions?> DeleteRolePermissionAsync(RolePermissionId rolePermissionId)
        {
            var existingRolePermissions = await _dbContext.RolePermissions.FindAsync(rolePermissionId);

            if (existingRolePermissions != null)
            {
                _dbContext.RolePermissions.Remove(existingRolePermissions);
                await _dbContext.SaveEfContextChanges("");
            }

            return existingRolePermissions;
        }

       

        public async Task<RolePermissions?> UpdateRolePermissionAsync(RolePermissionId rolePermissionId, RolePermissions rolePermissions)
        {
            _dbContext.RolePermissions.Update(rolePermissions);
            await _dbContext.SaveEfContextChanges("");
            return rolePermissions;
        }

      

      



    }
}
