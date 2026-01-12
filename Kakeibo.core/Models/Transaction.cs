namespace Kakeibo.Core.Models;

public sealed record Transaction(
    int Id,
    DateOnly Date,
    decimal Amount,
    string Category,
    string? Memo
);
