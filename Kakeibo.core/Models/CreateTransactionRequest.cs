namespace Kakeibo.Core.Models;

public sealed record CreateTransactionRequest(
    DateOnly Date,
    decimal Amount,
    string Category,
    string? Memo
);
