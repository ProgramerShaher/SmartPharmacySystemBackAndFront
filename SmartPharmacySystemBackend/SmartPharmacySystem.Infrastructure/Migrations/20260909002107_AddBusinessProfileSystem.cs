using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessProfileSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessProfileId",
                table: "PharmacySettings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessType",
                table: "PharmacySettings",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "InternalCode",
                table: "Medicines",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCustom = table.Column<bool>(type: "bit", nullable: false),
                    TrackExpiryDate = table.Column<bool>(type: "bit", nullable: false),
                    TrackBatchNumber = table.Column<bool>(type: "bit", nullable: false),
                    TrackSerialNumbers = table.Column<bool>(type: "bit", nullable: false),
                    UseProductVariants = table.Column<bool>(type: "bit", nullable: false),
                    AllowDecimalQuantity = table.Column<bool>(type: "bit", nullable: false),
                    UseScaleBarcode = table.Column<bool>(type: "bit", nullable: false),
                    HasWarranty = table.Column<bool>(type: "bit", nullable: false),
                    HasAlternatives = table.Column<bool>(type: "bit", nullable: false),
                    TrackDimensions = table.Column<bool>(type: "bit", nullable: false),
                    TrackModelNumber = table.Column<bool>(type: "bit", nullable: false),
                    RequireBarcode = table.Column<bool>(type: "bit", nullable: false),
                    RequireCategory = table.Column<bool>(type: "bit", nullable: false),
                    RequireCustomer = table.Column<bool>(type: "bit", nullable: false),
                    AllowSellBelowCost = table.Column<bool>(type: "bit", nullable: false),
                    RequireShiftToSell = table.Column<bool>(type: "bit", nullable: false),
                    CheckCustomerCreditLimit = table.Column<bool>(type: "bit", nullable: false),
                    PreventCreditSaleWithoutCustomer = table.Column<bool>(type: "bit", nullable: false),
                    UseFEFO = table.Column<bool>(type: "bit", nullable: false),
                    AllowMultiPayment = table.Column<bool>(type: "bit", nullable: false),
                    AllowHoldInvoice = table.Column<bool>(type: "bit", nullable: false),
                    UseCashDrawer = table.Column<bool>(type: "bit", nullable: false),
                    DefaultPrintTemplate = table.Column<int>(type: "int", nullable: false),
                    RequirePurchaseOrder = table.Column<bool>(type: "bit", nullable: false),
                    UseLandedCost = table.Column<bool>(type: "bit", nullable: false),
                    EnableVAT = table.Column<bool>(type: "bit", nullable: false),
                    DefaultVATRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_BusinessProfiles", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(2146));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3199));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3201));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3202));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3224));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3225));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3227));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3228));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 58, 567, DateTimeKind.Utc).AddTicks(3288));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 59, 446, DateTimeKind.Utc).AddTicks(2291));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 9, 0, 20, 59, 446, DateTimeKind.Utc).AddTicks(3840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$q2vnTWyGHQ07u9SW3dDkpuMOmaDgFxx3OLw1GDkcGb.xiGahGLTSm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$q2vnTWyGHQ07u9SW3dDkpuMOmaDgFxx3OLw1GDkcGb.xiGahGLTSm");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Phone_Lookup",
                table: "Suppliers",
                columns: new[] { "PhoneNumber", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacySettings_BusinessProfileId",
                table: "PharmacySettings",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Barcode_Lookup",
                table: "Medicines",
                columns: new[] { "DefaultBarcode", "IsDeleted", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_InternalCode_Lookup",
                table: "Medicines",
                columns: new[] { "InternalCode", "IsDeleted", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone_Lookup",
                table: "Customers",
                columns: new[] { "PhoneNumber", "IsDeleted", "IsActive" });

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacySettings_BusinessProfiles_BusinessProfileId",
                table: "PharmacySettings",
                column: "BusinessProfileId",
                principalTable: "BusinessProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacySettings_BusinessProfiles_BusinessProfileId",
                table: "PharmacySettings");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Phone_Lookup",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacySettings_BusinessProfileId",
                table: "PharmacySettings");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_Barcode_Lookup",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_InternalCode_Lookup",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Phone_Lookup",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BusinessProfileId",
                table: "PharmacySettings");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "PharmacySettings");

            migrationBuilder.AlterColumn<string>(
                name: "InternalCode",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 775, DateTimeKind.Utc).AddTicks(7428));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(8887));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(8894));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9120));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9123));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9124));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9126));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9128));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 20, 776, DateTimeKind.Utc).AddTicks(9129));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 22, 368, DateTimeKind.Utc).AddTicks(4664));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 9, 5, 19, 54, 22, 368, DateTimeKind.Utc).AddTicks(7188));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$GoPK5rRWOeWqBtbg/pl7POUvtAyPfU1lYeIztvh0YQPNW6MwF0RxO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$GoPK5rRWOeWqBtbg/pl7POUvtAyPfU1lYeIztvh0YQPNW6MwF0RxO");
        }
    }
}
