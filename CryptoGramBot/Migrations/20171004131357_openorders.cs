using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CryptoGramBot.Migrations
{
    public partial class openorders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Base = table.Column<string>(type: "TEXT", nullable: true),
                    CancelInitiated = table.Column<bool>(type: "INTEGER", nullable: false),
                    CommissionPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    Condition = table.Column<string>(type: "TEXT", nullable: true),
                    ConditionTarget = table.Column<string>(type: "TEXT", nullable: true),
                    Exchange = table.Column<string>(type: "TEXT", nullable: true),
                    ImmediateOrCancel = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsConditional = table.Column<bool>(type: "INTEGER", nullable: false),
                    Limit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Opened = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OrderUuid = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "TEXT", nullable: false),
                    Side = table.Column<int>(type: "INTEGER", nullable: false),
                    Terms = table.Column<string>(type: "TEXT", nullable: true),
                    Uuid = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenOrders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpenOrders_Id",
                table: "OpenOrders",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenOrders");
        }
    }
}
