using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Data.Entities;

public sealed class TransactionEntity
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [MaxLength(100)]
    public required string Category { get; set; }

    [MaxLength(500)]
    public string? Memo { get; set; }
}
