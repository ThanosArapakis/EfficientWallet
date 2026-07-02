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
    }
}
