using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomatedAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomatedAuditHeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CountType = table.Column<int>(type: "int", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TotalOpeningValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPurchasesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSalesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDamagesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalShortageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_AutomatedAuditHeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditHeaders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditHeaders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditHeaders_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutomatedAuditItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutomatedAuditHeaderId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    OpeningBalance = table.Column<int>(type: "int", nullable: false),
                    TotalPurchases = table.Column<int>(type: "int", nullable: false),
                    TotalSales = table.Column<int>(type: "int", nullable: false),
                    TotalTransfersIn = table.Column<int>(type: "int", nullable: false),
                    TotalTransfersOut = table.Column<int>(type: "int", nullable: false),
                    TotalDamages = table.Column<int>(type: "int", nullable: false),
                    TotalAdjustments = table.Column<int>(type: "int", nullable: false),
                    TotalSalesReturns = table.Column<int>(type: "int", nullable: false),
                    TotalPurchaseReturns = table.Column<int>(type: "int", nullable: false),
                    ExpectedSystemBalance = table.Column<int>(type: "int", nullable: false),
                    ActualSystemBalance = table.Column<int>(type: "int", nullable: false),
                    Variance = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VarianceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_AutomatedAuditItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditItems_AutomatedAuditHeaders_AutomatedAuditHeaderId",
                        column: x => x.AutomatedAuditHeaderId,
                        principalTable: "AutomatedAuditHeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditItems_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AutomatedAuditItems_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 379, DateTimeKind.Utc).AddTicks(8676));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(166));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(169));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(170));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(172));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(173));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(175));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(176));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 47, 380, DateTimeKind.Utc).AddTicks(177));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 48, 65, DateTimeKind.Utc).AddTicks(2509));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 21, 23, 48, 65, DateTimeKind.Utc).AddTicks(8259));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bIe5PsQANly9lX8WffykveXh6pCyLHeSZDUEg5.zucM.VTJe.V9xS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$bIe5PsQANly9lX8WffykveXh6pCyLHeSZDUEg5.zucM.VTJe.V9xS");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditHeaders_BranchId",
                table: "AutomatedAuditHeaders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditHeaders_CreatedByUserId",
                table: "AutomatedAuditHeaders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditHeaders_WarehouseId",
                table: "AutomatedAuditHeaders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditItems_AutomatedAuditHeaderId",
                table: "AutomatedAuditItems",
                column: "AutomatedAuditHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditItems_BatchId",
                table: "AutomatedAuditItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditItems_MedicineId",
                table: "AutomatedAuditItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_AutomatedAuditItems_WarehouseId",
                table: "AutomatedAuditItems",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomatedAuditItems");

            migrationBuilder.DropTable(
                name: "AutomatedAuditHeaders");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(4157));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5301));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5303));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5304));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5305));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5306));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5307));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5308));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 27, 562, DateTimeKind.Utc).AddTicks(5309));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 28, 147, DateTimeKind.Utc).AddTicks(4496));

            migrationBuilder.UpdateData(
                table: "JournalEntryLines",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 19, 29, 28, 147, DateTimeKind.Utc).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$AFCKYpU9fpK6RRN3xCD3K.dMAvj5bb.ZsZR3a4bUvEGAI01rqrCda");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$AFCKYpU9fpK6RRN3xCD3K.dMAvj5bb.ZsZR3a4bUvEGAI01rqrCda");
        }
    }
}
