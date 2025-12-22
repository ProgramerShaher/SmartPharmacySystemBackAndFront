using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RandomIvoicesNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SaleInvoiceNumber",
                table: "SaleInvoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseInvoiceNumber",
                table: "PurchaseInvoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InvoiceSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoices_SaleInvoiceNumber",
                table: "SaleInvoices",
                column: "SaleInvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PurchaseInvoiceNumber",
                table: "PurchaseInvoices",
                column: "PurchaseInvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSequences_Prefix_Year",
                table: "InvoiceSequences",
                columns: new[] { "Prefix", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceSequences");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoices_SaleInvoiceNumber",
                table: "SaleInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_PurchaseInvoiceNumber",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceNumber",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceNumber",
                table: "PurchaseInvoices");
        }
    }
}
