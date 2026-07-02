using System.Data;
using System.Text;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EfficientWallet.Infrastructure.Persistence.Repository
{
    /// <summary>
    /// Repository for the <see cref="ExchangeRate"/> entity. Inherits the common
    /// CRUD behaviour from <see cref="Repository{TEntity}"/> and adds the queries
    /// that are specific to exchange rates.
    /// </summary>
    public class ExchangeRateRepository : Repository<ExchangeRate>, IExchangeRateRepository
    {
        public ExchangeRateRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Bulk-upserts the given rates in a single MERGE statement (one transaction).
        /// </summary>
        public async Task<int> UpsertAsync(IEnumerable<ExchangeRate> rates, CancellationToken cancellationToken = default)
        {
            var incoming = rates.ToList();
            if (incoming.Count == 0)
                return 0;

            // Build one parameterised VALUES row per rate: (@d0,@c0,@r0,@b0), (@d1,...), ...
            // Only the placeholder names are interpolated; every value is a parameter.
            var rows = new StringBuilder();
            var parameters = new List<object>(incoming.Count * 4);

            for (int i = 0; i < incoming.Count; i++)
            {
                var rate = incoming[i];

                if (i > 0)
                    rows.Append(", ");
                rows.Append($"(@d{i}, @c{i}, @r{i}, @b{i})");

                parameters.Add(new SqlParameter($"@d{i}", SqlDbType.Date) { Value = rate.RatesDate.ToDateTime(TimeOnly.MinValue) });
                parameters.Add(new SqlParameter($"@c{i}", SqlDbType.NChar, 3) { Value = rate.Currency });
                parameters.Add(new SqlParameter($"@r{i}", SqlDbType.Decimal) { Precision = 18, Scale = 6, Value = rate.Rate });
                parameters.Add(new SqlParameter($"@b{i}", SqlDbType.NVarChar) { Value = rate.BaseCurrency });
            }

            var sql = $@"
                MERGE dbo.ExchangeRates AS target
                USING (VALUES {rows}) AS source (RatesDate, Currency, Rate, BaseCurrency)
                    ON  target.RatesDate = source.RatesDate
                    AND target.Currency  = source.Currency
                WHEN MATCHED AND target.Rate <> source.Rate THEN
                    UPDATE SET target.Rate      = source.Rate,
                               target.UpdatedAt = SYSUTCDATETIME()
                WHEN NOT MATCHED BY TARGET THEN
                    INSERT (Id, RatesDate, Currency, Rate, BaseCurrency, CreatedAt, UpdatedAt)
                    VALUES (NEWID(), source.RatesDate, source.Currency, source.Rate,
                            source.BaseCurrency, SYSUTCDATETIME(), SYSUTCDATETIME());";

            return await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }
    }
}
