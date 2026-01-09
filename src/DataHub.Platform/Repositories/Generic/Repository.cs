using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DataHub.Core.Interfaces;

namespace DataHub.Platform.Repositories.Generic
{
    public class Repository<TEntity, TContext> : IRepository<TEntity> 
        where TEntity : class
        where TContext : DbContext
    {
        private readonly IDbContextFactory<TContext> _dbContextFactory;

        public Repository(IDbContextFactory<TContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            // For IQueryable, we create a context, but it's up to the consumer to dispose it
            // or materialize the query within a proper scope.
            // This is a common pattern for IQueryable return types with DbContextFactory.
            return _dbContextFactory.CreateDbContext().Set<TEntity>().AsNoTracking();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Set<TEntity>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Set<TEntity>().Update(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity != null)
            {
                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();
            }
        }
    }
}
