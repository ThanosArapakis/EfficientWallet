using EfficientWallet.Application.Common;
using EfficientWallet.Application.Common.Contracts;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance;
using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using EfficientWallet.Domain.Enums;
using EfficientWallet.Infrastructure.Migrations;
using EfficientWallet.Infrastructure.Persistence.Util;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Persistence.Repository;

public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    private readonly IBalanceConverter _balanceConverter;


    public WalletRepository(AppDbContext context, IBalanceConverter converter) : base(context)
    {
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
            return Error.Failure("Exception", ex.Message + ": " + ex.InnerException?.Message); 
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

    public async Task<ErrorOr<PostResponse>> AdjustWalletBalance(AdjustWalletBalanceCommand command, CancellationToken cancellationToken = default)
    {
        ErrorOr<bool> validate = command.Validate();
        if (!validate.IsSuccess && validate.Errors.Count > 0)
            return validate.Errors;

        try
        {
            Wallet? wallet = await GetByIdAsync(command.WalletId, cancellationToken);
            if (wallet == null) return Error.NotFound(description: "Wallet not found");

            switch (command.Strategy)
            {
                // AddFundsStrategy: Adds funds to the wallet balance after converting the amount to the wallet's currency.
                case (Strategy.AddFundsStrategy):
                    wallet.Balance += _balanceConverter.Convert(command.Currency, wallet.Currency, command.Amount);
                    break;
                // SubtractFundsStrategy: Subtracts funds from the wallet balance after converting the amount to the wallet's currency. If the wallet balance is insufficient, returns an error.
                case (Strategy.SubtractFundsStrategy):
                    decimal convertedAmount = _balanceConverter.Convert(command.Currency, wallet.Currency, command.Amount);
                    if (wallet.Balance < convertedAmount)
                        return CustomErrors.InsufficientAmount(wallet.Currency);
                    wallet.Balance -= convertedAmount;
                    break;
                // ForceSubtractFundsStrategy: Subtracts funds from the wallet balance after converting the amount to the wallet's currency, regardless of whether the wallet balance is sufficient or not.
                case (Strategy.ForceSubtractFundsStrategy):
                    wallet.Balance -= _balanceConverter.Convert(command.Currency, wallet.Currency, command.Amount);
                    break;
                default:    
                    return Error.Failure("Invalid Strategy", "The provided strategy is not valid.");
            }
            _dbSet.Update(wallet);
            await _context.SaveChangesAsync(cancellationToken);

            return new PostResponse(wallet.Id);
        }
        catch (Exception ex)
        {
            return Error.Failure("Database Exception", ex.Message + ": " + ex.InnerException?.Message);
        }
    }
}