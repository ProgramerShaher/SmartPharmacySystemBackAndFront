using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class pricelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscount",
                table: "SaleInvoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleInvoiceDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "SaleInvoiceDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PricelistId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pricelists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GlobalDiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricelists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricelistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PricelistId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    FixedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricelistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricelistItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PricelistItems_Pricelists_PricelistId",
                        column: x => x.PricelistId,
                        principalTable: "Pricelists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                column: "PricelistId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(4742));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5882));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5885));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5886));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5887));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5888));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5889));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5890));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 6, 978, DateTimeKind.Utc).AddTicks(5891));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 7, 685, DateTimeKind.Utc).AddTicks(2896));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 18, 19, 57, 7, 685, DateTimeKind.Utc).AddTicks(5499));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Y2Lf1r88293U4l8s78/YGOTfWKTsBR/.SsMUU0J3rcF5dJUop/YxS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Y2Lf1r88293U4l8s78/YGOTfWKTsBR/.SsMUU0J3rcF5dJUop/YxS");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PricelistId",
                table: "Customers",
                column: "PricelistId");

            migrationBuilder.CreateIndex(
                name: "IX_PricelistItems_MedicineId",
                table: "PricelistItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PricelistItems_PricelistId_MedicineId",
                table: "PricelistItems",
                columns: new[] { "PricelistId", "MedicineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pricelists_Name",
                table: "Pricelists",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Pricelists_PricelistId",
                table: "Customers",
                column: "PricelistId",
                principalTable: "Pricelists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Pricelists_PricelistId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "PricelistItems");

            migrationBuilder.DropTable(
                name: "Pricelists");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PricelistId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalDiscount",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "SaleInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "PricelistId",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(848));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(1973));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(1975));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(1976));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(1978));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(2067));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(2069));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 188, DateTimeKind.Utc).AddTicks(2071));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 867, DateTimeKind.Utc).AddTicks(3932));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 21, 3, 47, 867, DateTimeKind.Utc).AddTicks(6673));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$EMiXb67HTy99Lvj/zqWtCeP32oHrCWX/dK2ebeINuV9LzHgDsVj0.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$EMiXb67HTy99Lvj/zqWtCeP32oHrCWX/dK2ebeINuV9LzHgDsVj0.");
        }
    }
}
