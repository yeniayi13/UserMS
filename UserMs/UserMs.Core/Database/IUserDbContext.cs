using UserMs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.ActivityHistory;

namespace UserMs.Core.Database
{
    public interface IUserDbContext
    {
        DbContext DbContext { get; }
       
        DbSet<Users> Users { get; set; }
        DbSet<Roles> Roles { get; set; }
        DbSet<UserRoles> UserRoles { get; set; }
        DbSet<RolePermissions> RolePermissions { get; set; }
        DbSet<Permissions> Permissions { get; set; }
        DbSet<ActivityHistory> ActivityHistories { get; set; }
        DbSet<Supports> Supports { get; set; }
        DbSet<Auctioneers> Auctioneers { get; set; }

        DbSet<Bidders> Bidders { get; set; }




        IDbContextTransactionProxy BeginTransaction();

        void ChangeEntityState<TEntity>(TEntity entity, EntityState state);

        Task<bool> SaveEfContextChanges(string user, CancellationToken cancellationToken = default);
    }
}