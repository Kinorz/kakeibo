using Kakeibo.Core.Models;

namespace Kakeibo.Core.Services;

public interface ITransactionService
{
    Task<IReadOnlyList<Transaction>> GetAllAsync(DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Transaction> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
