using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MedicienUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseUnitName",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MedicineUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConversionFactor = table.Column<int>(type: "int", nullable: false),
                    DefaultPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DefaultSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_MedicineUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineUnits_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(129));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2516));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2521));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2523));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2527));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2708));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2710));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2712));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 20, 756, DateTimeKind.Utc).AddTicks(2714));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 22, 79, DateTimeKind.Utc).AddTicks(8584));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 24, 14, 49, 22, 81, DateTimeKind.Utc).AddTicks(2273));

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseUnitName",
                value: "حبة");

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseUnitName",
                value: "حبة");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Tq8UmC7naRaCdQpMq6.NLO8PxIwXML9Armd5gQhTZCk9wJ5w9Uywq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$Tq8UmC7naRaCdQpMq6.NLO8PxIwXML9Armd5gQhTZCk9wJ5w9Uywq");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineUnits_Barcode",
                table: "MedicineUnits",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineUnits_MedicineId",
                table: "MedicineUnits",
                column: "MedicineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicineUnits");

            migrationBuilder.DropColumn(
                name: "BaseUnitName",
                table: "Medicines");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(4473));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6149));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6153));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6155));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6156));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6158));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6160));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6162));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 187, DateTimeKind.Utc).AddTicks(6207));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 902, DateTimeKind.Utc).AddTicks(7339));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 7, 22, 13, 56, 27, 903, DateTimeKind.Utc).AddTicks(1060));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$0YOrmeE3d.ykkyduYA.LKujy7a6r/TwCL6gFVlerD1NqjrxOXTRpS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$0YOrmeE3d.ykkyduYA.LKujy7a6r/TwCL6gFVlerD1NqjrxOXTRpS");
        }
    }
}
