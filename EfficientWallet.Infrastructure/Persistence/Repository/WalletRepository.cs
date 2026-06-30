using EfficientWallet.Application.Common;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using EfficientWallet.Infrastructure.Migrations;
using EfficientWallet.Infrastructure.Persistence.Util;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ReservationApp.core.api.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Persistence.Repository;

public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    private readonly IRateCache _cache;
    private readonly IBalanceConverter _balanceConverter;

    public WalletRepository(AppDbContext context, IRateCache cache, IBalanceConverter converter) : base(context)
    {
        _cache = cache;
        _balanceConverter = converter;
    }

    public async Task<ErrorOr<RetrieveWalletBalanceResponse>> RetrieveWalletBalanceAsync(RetrieveWalletBalanceQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            Wallet? wallet = await GetByIdAsync(query.WalletId, cancellationToken);
            if (wallet == null) return Error.NotFound(description: "Wallet not found");

            return new RetrieveWalletBalanceResponse(_balanceConverter.Convert(wallet.Currency, query.Currency, wallet.Balance));
        }
        catch (Exception ex) 
        { 
            return Error.Failure("Database Exception", ex.Message + ": " + ex.InnerException?.Message); 
        }
    }

    public async Task<ErrorOr<PostResponse>> CreateWallet(CreateWalletCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            Wallet wallet = new Wallet { Balance = command.Balance, Currency = command.Currency };

            await _dbSet.AddAsync(wallet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new PostResponse(wallet.Id);
        }
        catch (Exception ex)
        {
            return Error.Failure("Database Exception", ex.Message + ": " + ex.InnerException?.Message);
        }
    }
}