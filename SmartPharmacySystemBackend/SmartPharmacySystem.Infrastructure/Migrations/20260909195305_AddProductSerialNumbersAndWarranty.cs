using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSerialNumbersAndWarranty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultWarrantyMonths",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProductSerialNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PurchaseInvoiceDetailId = table.Column<int>(type: "int", nullable: true),
                    SaleInvoiceDetailId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyMonths = table.Column<int>(type: "int", nullable: false),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ProductSerialNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_PurchaseInvoiceDetails_PurchaseInvoiceDetailId",
                        column: x => x.PurchaseInvoiceDetailId,
                        principalTable: "PurchaseInvoiceDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSerialNumbers_SaleInvoiceDetails_SaleInvoiceDetailId",
                        column: x => x.SaleInvoiceDetailId,
                        principalTable: "SaleInvoiceDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 500, DateTimeKind.Utc).AddTicks(9130));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 500, DateTimeKind.Utc).AddTicks(9995));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 500, DateTimeKind.Utc).AddTicks(9997));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 500, DateTimeKind.Utc).AddTicks(9998));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 500, DateTimeKind.Utc).AddTicks(9999));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 501, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 501, DateTimeKind.Utc).AddTicks(27));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 501, DateTimeKind.Utc).AddTicks(29));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 0, 501, DateTimeKind.Utc).AddTicks(30));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 1, 342, DateTimeKind.Utc).AddTicks(959));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 19, 53, 1, 342, DateTimeKind.Utc).AddTicks(3803));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$LzO9W12Rq2.KTLx4UlmwEeRwR9i3FicJZEjWDJ..LdZcvNKR7tB/K");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$LzO9W12Rq2.KTLx4UlmwEeRwR9i3FicJZEjWDJ..LdZcvNKR7tB/K");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_BatchId",
                table: "ProductSerialNumbers",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_CustomerId",
                table: "ProductSerialNumbers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_MedicineId_Status",
                table: "ProductSerialNumbers",
                columns: new[] { "MedicineId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_PurchaseInvoiceDetailId",
                table: "ProductSerialNumbers",
                column: "PurchaseInvoiceDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_SaleInvoiceDetailId",
                table: "ProductSerialNumbers",
                column: "SaleInvoiceDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSerialNumbers_SerialNumber",
                table: "ProductSerialNumbers",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSerialNumbers");

            migrationBuilder.DropColumn(
                name: "DefaultWarrantyMonths",
                table: "Medicines");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(1034));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2255));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2258));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2260));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2262));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2264));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2266));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2268));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 58, 164, DateTimeKind.Utc).AddTicks(2270));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 59, 159, DateTimeKind.Utc).AddTicks(7891));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 10, 46, 59, 159, DateTimeKind.Utc).AddTicks(9513));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uuvDrfUuWpEpvED5OTihxeaxZ/XgtnTVi15b.YNJZvK9nx5ArE98C");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$uuvDrfUuWpEpvED5OTihxeaxZ/XgtnTVi15b.YNJZvK9nx5ArE98C");
        }
    }
}
