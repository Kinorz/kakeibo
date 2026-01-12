using Kakeibo.Core.Data;
using Kakeibo.Core.Data.Entities;
using Kakeibo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Handlers;

public sealed class EfTransactionHandler : ITransactionHandler
{
    private readonly KakeiboDbContext _db;

    public EfTransactionHandler(KakeiboDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TransactionEntity> query = _db.Transactions.AsNoTracking();

        if (from is not null)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new Transaction(x.Id, x.Date, x.Amount, x.Category, x.Memo))
            .ToArray();
    }

    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return row is null ? null : new Transaction(row.Id, row.Date, row.Amount, row.Category, row.Memo);
    }

    public async Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TransactionEntity
        {
            Date = request.Date,
            Amount = request.Amount,
            Category = request.Category,
            Memo = request.Memo
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new Transaction(entity.Id, entity.Date, entity.Amount, entity.Category, entity.Memo);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Transactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _db.Transactions.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
