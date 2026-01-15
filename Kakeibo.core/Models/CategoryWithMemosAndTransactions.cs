using Kakeibo.Core.Data.Entities;

namespace Kakeibo.Core.Models;

public sealed record CategoryWithMemosAndTransactions(
    CategoryEntity Category,
    string Memos,
    IEnumerable<TransactionEntity> Transactions);
