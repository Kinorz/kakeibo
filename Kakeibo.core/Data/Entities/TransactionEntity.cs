using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Kakeibo.Core.Data.Entities;

public sealed class TransactionEntity
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public int CategoryId { get; set; }

    public CategoryEntity? Category { get; set; }

    [MaxLength(500)]
    public string? Memo { get; set; }
}
