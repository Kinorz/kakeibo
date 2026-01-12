using Kakeibo.Core.Models;
using Kakeibo.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kakeibo.Tests;

[Collection("SqlServerLocalDb")]
public sealed class TransactionServiceTests
{
    [Fact]
    public async Task CreateAsync_amount_zero_throws()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var req = new CreateTransactionRequest(
            Date: new DateOnly(2026, 1, 13),
            Amount: 0,
            Category: "Food",
            Memo: null
        );

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(req));
    }

    [Fact]
    public async Task CreateAsync_category_empty_throws()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var req = new CreateTransactionRequest(
            Date: new DateOnly(2026, 1, 13),
            Amount: -100,
            Category: " ",
            Memo: null
        );

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(req));
    }

    [Fact]
    public async Task Crud_flow_works_with_development_localdb()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var req = new CreateTransactionRequest(
            Date: new DateOnly(2026, 1, 13),
            Amount: -980,
            Category: "Groceries",
            Memo: "Milk"
        );

        var created = await service.CreateAsync(req);

        Assert.True(created.Id > 0);

        var got = await service.GetByIdAsync(created.Id);
        Assert.NotNull(got);
        Assert.Equal(created.Id, got!.Id);
        Assert.Equal(req.Date, got.Date);
        Assert.Equal(req.Amount, got.Amount);
        Assert.Equal(req.Category, got.Category);
        Assert.Equal(req.Memo, got.Memo);

        var all = await service.GetAllAsync();
        Assert.Contains(all, x => x.Id == created.Id);

        var deleted = await service.DeleteAsync(created.Id);
        Assert.True(deleted);

        var afterDelete = await service.GetByIdAsync(created.Id);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task GetAllAsync_respects_from_to_filters()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var createdIds = new List<int>();
        try
        {
            var t1 = await service.CreateAsync(new CreateTransactionRequest(new DateOnly(2026, 1, 1), -100, "Food", null));
            var t2 = await service.CreateAsync(new CreateTransactionRequest(new DateOnly(2026, 1, 15), -200, "Food", null));
            var t3 = await service.CreateAsync(new CreateTransactionRequest(new DateOnly(2026, 2, 1), -300, "Food", null));
            createdIds.AddRange(new[] { t1.Id, t2.Id, t3.Id });

            var from = new DateOnly(2026, 1, 1);
            var to = new DateOnly(2026, 1, 31);
            var result = await service.GetAllAsync(from, to);

            Assert.True(result.Count >= 2);
            Assert.All(result, x => Assert.InRange(x.Date, from, to));
        }
        finally
        {
            foreach (var id in createdIds)
            {
                await service.DeleteAsync(id);
            }
        }
    }
}
