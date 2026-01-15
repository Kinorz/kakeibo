using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kakeibo.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories",
                column: "CategoryName",
                unique: true);

            // Add FK column as nullable first so we can backfill it from the old string column.
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "int",
                nullable: true);

            // Ensure a fallback category exists.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [CategoryName] = N'Uncategorized')
BEGIN
    INSERT INTO [Categories] ([CategoryName]) VALUES (N'Uncategorized');
END
");

            // Create categories from existing transaction values.
            // Note: [Category] column exists at this point (from InitialCreate migration).
            migrationBuilder.Sql(@"
INSERT INTO [Categories] ([CategoryName])
SELECT DISTINCT LTRIM(RTRIM([t].[Category]))
FROM [Transactions] AS [t]
WHERE LTRIM(RTRIM([t].[Category])) <> N''
  AND NOT EXISTS (
        SELECT 1 FROM [Categories] AS [c]
        WHERE [c].[CategoryName] = LTRIM(RTRIM([t].[Category]))
  );
");

            // Backfill Transactions.CategoryId from Categories.CategoryName.
            migrationBuilder.Sql(@"
UPDATE [t]
SET [t].[CategoryId] = [c].[CategoryId]
FROM [Transactions] AS [t]
INNER JOIN [Categories] AS [c]
    ON [c].[CategoryName] = LTRIM(RTRIM([t].[Category]));
");

            // Any remaining NULLs (blank categories) become Uncategorized.
            migrationBuilder.Sql(@"
UPDATE [Transactions]
SET [CategoryId] = (SELECT TOP(1) [CategoryId] FROM [Categories] WHERE [CategoryName] = N'Uncategorized')
WHERE [CategoryId] IS NULL;
");

            // Make CategoryId required now that it is populated.
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            // Drop the old denormalized string column after successful backfill.
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add denormalized column and backfill from Categories.
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE [t]
SET [t].[Category] = [c].[CategoryName]
FROM [Transactions] AS [t]
LEFT JOIN [Categories] AS [c]
    ON [c].[CategoryId] = [t].[CategoryId];
");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
