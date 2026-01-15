using Kakeibo.Core.Data;
using Kakeibo.Core.Data.Entities;
using Kakeibo.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Handlers;

public sealed class EfTransactionHandler
{
    private readonly KakeiboDbContext _db;

    public EfTransactionHandler(KakeiboDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TransactionEntity> query = _db.Transactions
            .AsNoTracking()
            .Include(x => x.Category);

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
            .Select(x => new Transaction(
                x.Id,
                x.Date,
                x.Amount,
                x.Category?.CategoryName ?? string.Empty,
                x.Memo))
            .ToArray();
    }

    public async Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await _db.Transactions
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return row is null
            ? null
            : new Transaction(row.Id, row.Date, row.Amount, row.Category?.CategoryName ?? string.Empty, row.Memo);
    }

    public async Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var categoryName = request.Category.Trim();

        var category = await _db.Categories
            .FirstOrDefaultAsync(x => x.CategoryName == categoryName, cancellationToken);

        if (category is null)
        {
            category = new CategoryEntity
            {
                CategoryName = categoryName
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var entity = new TransactionEntity
        {
            Date = request.Date,
            Amount = request.Amount,
            CategoryId = category.CategoryId,
            Memo = request.Memo
        };

        _db.Transactions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new Transaction(entity.Id, entity.Date, entity.Amount, category.CategoryName, entity.Memo);
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

    public async Task<IReadOnlyList<CategoryWithMemosAndTransactions>?> GroupJoinPractice(CancellationToken cancellationToken = default)
    {
        var memoByCategory = _db.Transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Memos = string.Join(",", g.Select(t => t.Memo))
            });

        var query = _db.Categories
            .GroupJoin(
                _db.Transactions,
                category => category.CategoryId,
                transaction => transaction.CategoryId,
                (category, transactions) => new { category, transactions })
            .Join(
                memoByCategory,
                x => x.category.CategoryId,
                memo => memo.CategoryId,
                (x, memo) => new
                {
                    x.category,
                    x.transactions,
                    Memos = memo.Memos
                }
            )
            .OrderBy(x => x.Memos)
            .ThenBy(x => x.category.CategoryId);

        var results = await query.ToListAsync(cancellationToken);
        return null;
    }
}
