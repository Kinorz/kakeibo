using System.Collections.Concurrent;
using System.Threading;
using Kakeibo.Core.Models;

namespace Kakeibo.Core.Handlers;

public sealed class InMemoryTransactionHandler : ITransactionHandler
{
    private readonly ConcurrentDictionary<int, Transaction> _store = new();
    private int _lastId;

    public Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<Transaction> query = _store.Values;

        if (from is not null)
        {
            query = query.Where(x => x.Date >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(x => x.Date <= to.Value);
        }

        var result = query
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToArray();

        return Task.FromResult<IReadOnlyList<Transaction>>(result);
    }

    public Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = Interlocked.Increment(ref _lastId);
        var created = new Transaction(
            Id: id,
            Date: request.Date,
            Amount: request.Amount,
            Category: request.Category,
            Memo: request.Memo
        );

        _store[id] = created;
        return Task.FromResult(created);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryRemove(id, out _));
    }
}
