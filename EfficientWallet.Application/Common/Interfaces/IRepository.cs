using ErrorOr;
using System.Linq.Expressions;

namespace EfficientWallet.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction over the common data-access behaviour shared by all entity
    /// repositories. Implemented in the Infrastructure layer so the rest of the
    /// application depends on this port rather than on EF Core directly.
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
