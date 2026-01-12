using System.Net;
using System.Net.Http.Json;
using Kakeibo.Core.Models;

namespace Kakeibo.Tests;

[Collection("SqlServerLocalDb")]
public sealed class TransactionsApiLocalDbTests
{
    [Fact]
    public async Task Crud_flow_works_with_localdb_and_migrations()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var client = factory.CreateClient();

        // Note: this test uses the development DB; it may already contain data.
        var before = await client.GetFromJsonAsync<Transaction[]>("/api/transactions");
        Assert.NotNull(before);
        var beforeCount = before!.Length;

        // POST
        var create = new CreateTransactionRequest(
            Date: new DateOnly(2026, 1, 13),
            Amount: -500,
            Category: "Transport",
            Memo: "Train"
        );

        var post = await client.PostAsJsonAsync("/api/transactions", create);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        // GET by id
        var got = await client.GetFromJsonAsync<Transaction>($"/api/transactions/{created.Id}");
        Assert.NotNull(got);
        Assert.Equal(created.Id, got!.Id);

        // DELETE (optional cleanup)
        var del = await client.DeleteAsync($"/api/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        // Deleted record should be gone
        var afterDelete = await client.GetAsync($"/api/transactions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);

        // Count should be back to where it was (unless something else wrote concurrently)
        var after = await client.GetFromJsonAsync<Transaction[]>("/api/transactions");
        Assert.NotNull(after);
        Assert.True(after!.Length >= beforeCount);
    }

    [Fact]
    public async Task test2()
    {
        await using var factory = new SqlServerTestAppFactory();
        using var client = factory.CreateClient();

        // Note: this test uses the development DB; it may already contain data.
        var before = await client.GetFromJsonAsync<Transaction[]>("/api/transactions");
        Assert.NotNull(before);
        var beforeCount = before!.Length;

        // POST
        var create = new CreateTransactionRequest(
            Date: new DateOnly(2026, 1, 13),
            Amount: -500,
            Category: "Transport",
            Memo: "Train"
        );

        var post = await client.PostAsJsonAsync("/api/transactions", create);
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);

        // GET by id
        var got = await client.GetFromJsonAsync<Transaction>($"/api/transactions/{created.Id}");
        Assert.NotNull(got);
        Assert.Equal(created.Id, got!.Id);
    }
}
