using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashTransferFieldsToUserShifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCashTransferredToMainSafe",
                table: "UserShifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CashTransferredAt",
                table: "UserShifts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferredCashAmount",
                table: "UserShifts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReferenceNumber",
                table: "UserShifts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCashTransferredToMainSafe",
                table: "UserShifts");

            migrationBuilder.DropColumn(
                name: "CashTransferredAt",
                table: "UserShifts");

            migrationBuilder.DropColumn(
                name: "TransferredCashAmount",
                table: "UserShifts");

            migrationBuilder.DropColumn(
                name: "TransferReferenceNumber",
                table: "UserShifts");
        }
    }
}
