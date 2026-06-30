using System.Linq.Expressions;
using EfficientWallet.Application.Common.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace EfficientWallet.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Generic repository that encapsulates the common EF Core data-access
    /// behaviour shared by all entity-specific repositories. Derive from this
    /// class to add entity-specific queries while reusing the CRUD plumbing.
    /// </summary>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
            => await _dbSet.FindAsync(new[] { id }, cancellationToken);

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _dbSet.ToListAsync(cancellationToken);

        public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            => await _dbSet.Where(predicate).ToListAsync(cancellationToken);

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            => await _dbSet.AddRangeAsync(entities, cancellationToken);

        public virtual void Update(TEntity entity) => _dbSet.Update(entity);

        public virtual void Remove(TEntity entity) => _dbSet.Remove(entity);

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}
