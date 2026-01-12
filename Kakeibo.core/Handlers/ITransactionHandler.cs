using Kakeibo.Core.Models;

namespace Kakeibo.Core.Handlers;

public interface ITransactionHandler
{
    Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
