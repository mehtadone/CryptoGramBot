using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CryptoGramBot.Migrations
{
    public partial class IdCOlumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposits_TransactionId",
                table: "Deposits");

            migrationBuilder.AddColumn<double>(
                name: "Cost",
                table: "Withdrawals",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Withdrawals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Cost",
                table: "Deposits",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_Id",
                table: "Deposits",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deposits_Id",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Withdrawals");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Withdrawals");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Deposits");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_TransactionId",
                table: "Deposits",
                column: "TransactionId",
                unique: true);
        }
    }
}
