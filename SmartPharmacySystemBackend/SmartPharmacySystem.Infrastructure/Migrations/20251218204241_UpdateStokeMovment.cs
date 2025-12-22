using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStokeMovment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingQuantity",
                table: "MedicineBatches");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SalesReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SaleInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PurchaseReturns",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PurchaseInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ReferenceId",
                table: "InventoryMovements",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "MovementType",
                table: "InventoryMovements",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReferenceType",
                table: "InventoryMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SaleInvoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "InventoryMovements");

            migrationBuilder.AddColumn<int>(
                name: "RemainingQuantity",
                table: "MedicineBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MovementType",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
