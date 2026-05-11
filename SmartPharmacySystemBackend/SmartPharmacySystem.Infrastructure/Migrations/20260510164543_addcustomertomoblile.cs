using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPharmacySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcustomertomoblile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OnlineOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CustomerNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    HandledBy = table.Column<int>(type: "int", nullable: true),
                    HandledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineOrders_Users_HandledBy",
                        column: x => x.HandledBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnlineOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlineOrderId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlineOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlineOrderItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlineOrderItems_OnlineOrders_OnlineOrderId",
                        column: x => x.OnlineOrderId,
                        principalTable: "OnlineOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(2636));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4577));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4581));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4583));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4584));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4585));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4586));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 10, 16, 45, 37, 386, DateTimeKind.Utc).AddTicks(4589));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ymQmErnZBKrcDjxAs7Ily.2dQdrNYiEOiFRip1HjpDr2EWkC5VOam");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ymQmErnZBKrcDjxAs7Ily.2dQdrNYiEOiFRip1HjpDr2EWkC5VOam");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrderItems_MedicineId",
                table: "OnlineOrderItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrderItems_OnlineOrderId",
                table: "OnlineOrderItems",
                column: "OnlineOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_CustomerId",
                table: "OnlineOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_HandledBy",
                table: "OnlineOrders",
                column: "HandledBy");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_OrderDate",
                table: "OnlineOrders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_OrderNumber",
                table: "OnlineOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnlineOrders_Status",
                table: "OnlineOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlineOrderItems");

            migrationBuilder.DropTable(
                name: "OnlineOrders");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Customers");

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 266, DateTimeKind.Utc).AddTicks(8661));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1550));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1555));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1556));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1616));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1619));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 8, 21, 41, 2, 267, DateTimeKind.Utc).AddTicks(1622));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$qW59dIuFs7CS/tCVjsefkuN1124X11Er6bCKxGckxYoIvhnc44SGO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$qW59dIuFs7CS/tCVjsefkuN1124X11Er6bCKxGckxYoIvhnc44SGO");
        }
    }
}
