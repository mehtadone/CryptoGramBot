using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CryptoGramBot.Migrations
{
    public partial class RealisedProfitColumn : Migration
    {
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealisedProfit",
                table: "ProfitAndLosses");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RealisedProfit",
                table: "ProfitAndLosses",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}