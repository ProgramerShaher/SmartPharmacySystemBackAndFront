using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoPartsCompatibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompatibleVehicles",
                table: "Medicines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerPartNumber",
                table: "Medicines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OemPartNumber",
                table: "Medicines",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(3228));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4486));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4490));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4492));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4494));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4561));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4563));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4565));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 6, 862, DateTimeKind.Utc).AddTicks(4567));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 7, 940, DateTimeKind.Utc).AddTicks(3950));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 21, 9, 7, 940, DateTimeKind.Utc).AddTicks(6188));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$dTbRyYphztMRyJ2veaGD1.wfrNIIuCHYx1swcwyy5.TqDfgpDn.vi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$dTbRyYphztMRyJ2veaGD1.wfrNIIuCHYx1swcwyy5.TqDfgpDn.vi");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_OemPartNumber",
                table: "Medicines",
                column: "OemPartNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Medicines_OemPartNumber",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "CompatibleVehicles",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ManufacturerPartNumber",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "OemPartNumber",
                table: "Medicines");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 246, DateTimeKind.Utc).AddTicks(8840));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(160));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(163));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(165));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(168));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(170));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(172));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(174));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 4, 247, DateTimeKind.Utc).AddTicks(176));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 5, 821, DateTimeKind.Utc).AddTicks(3412));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 20, 27, 5, 821, DateTimeKind.Utc).AddTicks(5998));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ZFdpf7L.wxAfnWoBYlCNqeBEprb.b31hc4crBJ4dKr8R6WXAI6PPC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ZFdpf7L.wxAfnWoBYlCNqeBEprb.b31hc4crBJ4dKr8R6WXAI6PPC");
        }
    }
}
