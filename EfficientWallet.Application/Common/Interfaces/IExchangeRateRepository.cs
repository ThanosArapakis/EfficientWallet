using EfficientWallet.Domain;

namespace EfficientWallet.Application.Common.Interfaces
{
    /// <summary>
    /// Repository abstraction for <see cref="ExchangeRate"/> entities, extending
    /// the generic <see cref="IRepository{TEntity}"/> with rate-specific queries.
    /// </summary>
    public interface IExchangeRateRepository : IRepository<ExchangeRate>
    {
        /// <summary>
        /// Bulk-upserts the given rates in a single MERGE statement (one transaction):
        /// inserts rows for new (date, currency) pairs and updates the rate when it
        /// has changed. Returns the number of rows affected.
        /// </summary>
        Task<int> UpsertAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default);
    }
}
