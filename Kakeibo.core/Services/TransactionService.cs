using Kakeibo.Core.Handlers;
using Kakeibo.Core.Models;

namespace Kakeibo.Core.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionHandler _handler;

    public TransactionService(ITransactionHandler handler)
    {
        _handler = handler;
    }

    public Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
        => _handler.GetAllAsync(from, to, cancellationToken);

    public Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _handler.GetByIdAsync(id, cancellationToken);

    public Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount == 0)
        {
            throw new ArgumentException("Amount must not be 0.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new ArgumentException("Category is required.", nameof(request));
        }

        return _handler.CreateAsync(request, cancellationToken);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _handler.DeleteAsync(id, cancellationToken);
}
