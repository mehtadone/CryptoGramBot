using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CryptoGramBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BalanceHistories",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DollarAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceHistories", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "LastCheckeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Exchange = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastCheckeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfitAndLosses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AverageBuyPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    AverageSellPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    Base = table.Column<string>(type: "TEXT", nullable: true),
                    CommissionPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    DollarProfit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Profit = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantityBought = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantitySold = table.Column<decimal>(type: "TEXT", nullable: false),
                    Terms = table.Column<string>(type: "TEXT", nullable: true),
                    UnrealisedProfit = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfitAndLosses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Base = table.Column<string>(type: "TEXT", nullable: true),
                    Commission = table.Column<decimal>(type: "TEXT", nullable: false),
                    Cost = table.Column<decimal>(type: "TEXT", nullable: false),
                    Exchange = table.Column<string>(type: "TEXT", nullable: true),
                    ExchangeId = table.Column<string>(type: "TEXT", nullable: true),
                    Limit = table.Column<decimal>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "TEXT", nullable: false),
                    Side = table.Column<int>(type: "INTEGER", nullable: false),
                    Terms = table.Column<string>(type: "TEXT", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WalletBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Address = table.Column<decimal>(type: "TEXT", nullable: false),
                    Available = table.Column<decimal>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    BtcAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: true),
                    Exchange = table.Column<string>(type: "TEXT", nullable: true),
                    Pending = table.Column<decimal>(type: "TEXT", nullable: false),
                    PercentageChange = table.Column<decimal>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Requested = table.Column<decimal>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Uuid = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBalances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceHistories_Key",
                table: "BalanceHistories",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LastCheckeds_Id",
                table: "LastCheckeds",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfitAndLosses_Id",
                table: "ProfitAndLosses",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trades_Id",
                table: "Trades",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_Id",
                table: "WalletBalances",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceHistories");

            migrationBuilder.DropTable(
                name: "LastCheckeds");

            migrationBuilder.DropTable(
                name: "ProfitAndLosses");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "WalletBalances");
        }
    }
}
