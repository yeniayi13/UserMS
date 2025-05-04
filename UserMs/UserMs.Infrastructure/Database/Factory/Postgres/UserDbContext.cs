using UserMs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using UserMs.Core.Database;
using EntityFramework.Exceptions.PostgreSQL;

namespace UserMs.Infrastructure.Database.Factory.Postgres
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

      
        public virtual DbSet<Users> Users { get; set; } = null!;

      

        public IDbContextTransactionProxy BeginTransaction()
        {
            return new DbContextTransactionProxy(this);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
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
                        .Property(d => d.UserDelete)
                        .HasConversion<UserDeleteConverter>();
            modelBuilder.Entity<Users>()   
                        .Property(u => u.UsersType);
            modelBuilder.Entity<Users>()
                       .Property(u => u.UserLastName)
                       .HasConversion(new UserLastNameValueConverter());


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
            return await SaveChangesAsync(user, cancellationToken) >= 0;
        }
    }
}