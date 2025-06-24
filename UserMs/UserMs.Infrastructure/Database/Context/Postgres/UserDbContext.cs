using UserMs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using UserMs.Core.Database;
using EntityFramework.Exceptions.PostgreSQL;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Permission.ValueConverter;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueConverter;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.Role.ValueConverter;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueConverter;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities.ActivityHistory;

namespace UserMs.Infrastructure.Database.Context.Postgress
{
    public class UserDbContext : DbContext, IUserDbContext
    {
        public UserDbContext(
           DbContextOptions<UserDbContext> options
       )
           : base(options) { }

        public DbContext DbContext
        {
            get { return this; }
        }
        public virtual DbSet<ActivityHistory> ActivityHistories { get; set; } = null!;

        public virtual DbSet<Users> Users { get; set; } = null!;
        public virtual DbSet<Roles> Roles { get; set; } = null!;
        public virtual DbSet<Permissions> Permissions { get; set; } = null!;
        public virtual DbSet<RolePermissions> RolePermissions { get; set; } = null!;
        public virtual DbSet<UserRoles> UserRoles { get; set; } = null!;
        public DbSet<Supports> Supports { get; set; } = null!;
        public DbSet<Auctioneers> Auctioneers { get; set; } = null!;
        public DbSet<Bidders> Bidders { get; set; } = null!;

        public IDbContextTransactionProxy BeginTransaction()
        {
            return new DbContextTransactionProxy(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ActivityHistory>()
                .Property(u => u.UserId)
                .HasConversion(new UserIdValueConverter());

            modelBuilder.Entity<Users>()   
                        .Property(u => u.UserId)   
                        .HasConversion(new UserIdValueConverter());
            modelBuilder.Entity<Users>()   
                        .Property(u => u.UserEmail)   
                        .HasConversion(new UserEmailValueConverter());
            modelBuilder.Entity<Users>()   
                        .Property(u => u.UserPassword)
                        .HasConversion(new UserPasswordValueConverter());
            modelBuilder.Entity<Users>()
                       .Property(u => u.UserName)
                       .HasConversion(new UserNameValueConverter());
            modelBuilder.Entity<Users>()
                       .Property(u => u.UserAddress)
                       .HasConversion(new UserAddressValueConverter());
            modelBuilder.Entity<Users>()
                       .Property(u => u.UserPhone)
                       .HasConversion(new UserPhoneValueConverter());
            modelBuilder.Entity<Users>()
                        .Property(u => u.UserDelete)
                        .HasConversion(new UserDeleteConverter()); 
            modelBuilder.Entity<Users>()   
                        .Property(u => u.UsersType);
            modelBuilder.Entity<Users>()
                       .Property(u => u.UserLastName)
                       .HasConversion(new UserLastNameValueConverter());

            modelBuilder.Entity<Users>().HasData(
                new
                {
                    UserId = Guid.Parse("7671574c-6fb8-43b7-98be-897a98c487a0"),
                    UsersType = UsersType.Administrador,
                    UserAvailable = UserAvailable.Activo,
                    UserEmail = "admin@example.com",
                    UserPassword = "hashedpassword",
                    UserDelete = false,
                    UserAddress = "123 Main St",
                    UserPhone = "555-1234",
                    UserName = "Admin",
                    UserLastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "seed",
                    UpdatedAt = (DateTime?)null,
                    UpdatedBy = (string)null
                }
            );


            modelBuilder.Entity<Roles>()
                .Property(r => r.RoleId)
                .HasConversion(new RoleConverter.RoleIdValueConverter());

            modelBuilder.Entity<Roles>()
                .Property(r => r.RoleName)
                .HasConversion(new RoleConverter.RoleNameValueConverter());

            modelBuilder.Entity<Roles>()
                .Property(r => r.IsDeleted)
                .HasConversion<RoleConverter.RoleDeleteConverter>();

            modelBuilder.Entity<Permissions>()
                .Property(p => p.PermissionId)
                .HasConversion(new PermissionConverter.PermissionIdValueConverter());

            modelBuilder.Entity<Permissions>()
                .Property(p => p.PermissionName)
                .HasConversion(new PermissionConverter.PermissionNameValueConverter());

            modelBuilder.Entity<RolePermissions>()
                .Property(rp => rp.PermissionId)
                .HasConversion(new PermissionConverter.PermissionIdValueConverter());


            modelBuilder.Entity<RolePermissions>()
                .Property(rp => rp.RoleId)
                .HasConversion(new RoleConverter.RoleIdValueConverter());

            modelBuilder.Entity<RolePermissions>()
                .Property(rp => rp.RolePermissionId)
                .HasConversion(new RolePermissionConverter.RolePermissionIdValueConverter());

            modelBuilder.Entity<UserRoles>()
                .Property(ur => ur.UserId)
                .HasConversion(new UserIdValueConverter());

            modelBuilder.Entity<UserRoles>()
                .Property(ur => ur.RoleId)
                .HasConversion(new RoleConverter.RoleIdValueConverter());

            modelBuilder.Entity<UserRoles>()
                .Property(ur => ur.UserRoleId)
                .HasConversion(new UserRoleConverter.UserRoleIdValueConverter());

            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserId)
                .HasConversion(new UserIdValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserEmail)
                .HasConversion(new UserEmailValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserPassword)
                .HasConversion(new UserPasswordValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserName)
                .HasConversion(new UserNameValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserAddress)
                .HasConversion(new UserAddressValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserPhone)
                .HasConversion(new UserPhoneValueConverter());
            modelBuilder.Entity<Auctioneers>()
                .Property(u => u.UserLastName)
                .HasConversion(new UserLastNameValueConverter());

            modelBuilder.Entity<Auctioneers>()
                .Property(a => a.AuctioneerDni)
                .HasConversion(new AuctioneerDniValueConverter());

            modelBuilder.Entity<Auctioneers>()
                .Property(a => a.AuctioneerBirthday)
                .HasConversion(new AuctioneerBirthdayValueConverter());

            modelBuilder.Entity<Auctioneers>()
                .Property(a => a.AuctioneerDelete)
                .HasConversion(new AuctioneerDeleteConverter());

            modelBuilder.Entity<Bidders>()
                .Property(b => b.BidderDelete)
                .HasConversion(new BidderDeleteConverter());

            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserId)
                .HasConversion(new UserIdValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserEmail)
                .HasConversion(new UserEmailValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserPassword)
                .HasConversion(new UserPasswordValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserName)
                .HasConversion(new UserNameValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserAddress)
                .HasConversion(new UserAddressValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserPhone)
                .HasConversion(new UserPhoneValueConverter());
            modelBuilder.Entity<Bidders>()
                .Property(u => u.UserLastName)
                .HasConversion(new UserLastNameValueConverter());

            modelBuilder.Entity<Bidders>()
                .Property(b => b.BidderDni)
                .HasConversion(new BidderDniValueConverter());

            modelBuilder.Entity<Bidders>()
                .Property(b => b.BidderBirthday)
                .HasConversion(new BidderBirthdayValueConverter());

            modelBuilder.Entity<Supports>()
                .Property(u => u.UserId)
                .HasConversion(new UserIdValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserEmail)
                .HasConversion(new UserEmailValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserPassword)
                .HasConversion(new UserPasswordValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserName)
                .HasConversion(new UserNameValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserAddress)
                .HasConversion(new UserAddressValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserPhone)
                .HasConversion(new UserPhoneValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(u => u.UserLastName)
                .HasConversion(new UserLastNameValueConverter());

            modelBuilder.Entity<Supports>()
                .Property(s => s.SupportDni)
                .HasConversion(new SupportDniValueConverter());
            modelBuilder.Entity<Supports>()
                .Property(s => s.SupportDelete)
                .HasConversion(new SupportDeleteConverter());

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor();
        }

        public virtual void SetPropertyIsModifiedToFalse<TEntity, TProperty>(
            TEntity entity,
            Expression<Func<TEntity, TProperty>> propertyExpression
        )
            where TEntity : class
        {
            Entry(entity).Property(propertyExpression).IsModified = false;
        }

        public virtual void ChangeEntityState<TEntity>(TEntity entity, EntityState state)
        {
            if (entity != null)
            {
                Entry(entity).State = state;
            }
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.Entity is Base
                    && (e.State == EntityState.Added || e.State == EntityState.Modified)
                );

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((Base)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                    ((Base)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
                }

                if (entityEntry.State == EntityState.Modified)
                {
                    ((Base)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
                    Entry((Base)entityEntry.Entity).Property(x => x.CreatedAt).IsModified =
                        false;
                    Entry((Base)entityEntry.Entity).Property(x => x.CreatedBy).IsModified =
                        false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(
            string user,
            CancellationToken cancellationToken = default
        )
        {
            var state = new List<EntityState> { EntityState.Added, EntityState.Modified };

            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Base && state.Any(s => e.State == s));

            var dt = DateTime.UtcNow;

            foreach (var entityEntry in entries)
            {
                var entity = (Base)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = dt;
                    entity.CreatedBy = user;
                    Entry(entity).Property(x => x.UpdatedAt).IsModified = false;
                    Entry(entity).Property(x => x.UpdatedBy).IsModified = false;
                }

                if (entityEntry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = dt;
                    entity.UpdatedBy = user;
                    Entry(entity).Property(x => x.CreatedAt).IsModified = false;
                    Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SaveEfContextChanges(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken) >= 0;
        }

        public async Task<bool> SaveEfContextChanges(
            string user,
            CancellationToken cancellationToken = default
        )
        {
            return await SaveChangesAsync(user, cancellationToken) >0;
        }
    }
}