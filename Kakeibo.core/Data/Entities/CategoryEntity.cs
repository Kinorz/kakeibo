using System.ComponentModel.DataAnnotations;

namespace Kakeibo.Core.Data.Entities;

public sealed class CategoryEntity
{
    public int CategoryId { get; set; }

    [MaxLength(100)]
    public required string CategoryName { get; set; }
}
