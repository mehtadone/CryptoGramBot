using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CryptoGramBot.Migrations
{
    public partial class ReportingCurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ProfitAndLosses: Migrate DollarProfit column to ReportingProfit column, add ReportingCurrency column defaulted to USD

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE ProfitAndLosses_temp AS SELECT * FROM ProfitAndLosses;
                  
                  DROP TABLE ProfitAndLosses;
                  
                  CREATE TABLE ProfitAndLosses (
                    Id                  INTEGER NOT NULL CONSTRAINT PK_ProfitAndLosses PRIMARY KEY AUTOINCREMENT,
                    AverageBuyPrice     TEXT NOT NULL,
                    AverageSellPrice    TEXT NOT NULL,
                    Base                TEXT NULL,
                    CommissionPaid      TEXT NOT NULL,
                    ReportingProfit     TEXT NOT NULL,
                    ReportingCurrency   TEXT NOT NULL,
                    Profit              TEXT NOT NULL,
                    QuantityBought      TEXT NOT NULL,
                    QuantitySold        TEXT NOT NULL,
                    Terms               TEXT NULL,
                    UnrealisedProfit    TEXT NOT NULL DEFAULT 0.0
                  );

                  CREATE UNIQUE INDEX IX_ProfitAndLosses_Id ON ProfitAndLosses (Id);

                  INSERT INTO ProfitAndLosses 
                  (
                    Id,
                    AverageBuyPrice,
                    AverageSellPrice,
                    Base,
                    CommissionPaid,
                    ReportingProfit,
                    ReportingCurrency,
                    Profit,
                    QuantityBought,
                    QuantitySold,
                    Terms,
                    UnrealisedProfit
                  )
                  SELECT Id,
                    AverageBuyPrice,
                    AverageSellPrice,
                    Base,
                    CommissionPaid,
                    DollarProfit,
                    'USD',
                    Profit,
                    QuantityBought,
                    QuantitySold,
                    Terms,
                    RealisedProfit
                  FROM ProfitAndLosses_temp;
                  
                  DROP TABLE ProfitAndLosses_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");


            // BalanceHistories: Migrate DollarAmount column to ReportingAmount column, add ReportingCurrency column defaulted to USD

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE BalanceHistories_temp AS SELECT * FROM BalanceHistories;
                  
                  DROP TABLE BalanceHistories;
                  
                  CREATE TABLE BalanceHistories (
                    Key                 INTEGER NOT NULL CONSTRAINT PK_BalanceHistories PRIMARY KEY AUTOINCREMENT,
                    Balance             TEXT NOT NULL,
                    DateTime            TEXT NOT NULL,
                    ReportingAmount     TEXT NOT NULL,
                    ReportingCurrency   TEXT NOT NULL,
                    Name                TEXT NULL
                  );
                  
                  CREATE UNIQUE INDEX IX_BalanceHistories_Key ON BalanceHistories (Key);

                  INSERT INTO BalanceHistories 
                  (
                    Key,
                    Balance,
                    DateTime,
                    ReportingAmount,
                    ReportingCurrency,
                    Name
                  )
                  SELECT Key,
                    Balance,
                    DateTime,
                    DollarAmount,
                    'USD',
                    Name
                  FROM BalanceHistories_temp;
                  
                  DROP TABLE BalanceHistories_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");


            // NOT MY CHANGES!!! Looks like model classes were updated without a db migration being created - this should bring the db up-to-date.

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE WalletBalances_temp AS SELECT * FROM WalletBalances;
                  
                  DROP TABLE WalletBalances;
                  
                  CREATE TABLE WalletBalances (
                    Id                  INTEGER NOT NULL CONSTRAINT PK_WalletBalances PRIMARY KEY AUTOINCREMENT,
                    Address             TEXT NULL,
                    Available           TEXT NOT NULL,
                    Balance             TEXT NOT NULL,
                    BtcAmount           TEXT NOT NULL,
                    Currency            TEXT NULL,
                    Exchange            TEXT NULL,
                    Pending             TEXT NOT NULL,
                    PercentageChange    TEXT NOT NULL,
                    Price               TEXT NOT NULL,
                    Requested           INTEGER NOT NULL,
                    Timestamp           TEXT NOT NULL,
                    Uuid                TEXT NULL
                  );
                  
                  CREATE UNIQUE INDEX IX_WalletBalances_Id ON WalletBalances (Id);

                  INSERT INTO WalletBalances 
                  (
                    Id,
                    Address,
                    Available,
                    Balance,
                    BtcAmount,
                    Currency,
                    Exchange,
                    Pending,
                    PercentageChange,
                    Price,
                    Requested,
                    Timestamp,
                    Uuid
                  )
                  SELECT Id,
                    NULL,
                    Available,
                    Balance,
                    BtcAmount,
                    Currency,
                    Exchange,
                    Pending,
                    PercentageChange,
                    Price,
                    0,
                    Timestamp,
                    Uuid
                  FROM WalletBalances_temp;
                  
                  DROP TABLE WalletBalances_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ProfitAndLosses: Migrate ReportingProfit column to DollarProfit column, drop ReportingProfit & ReportingCurrency columns

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE ProfitAndLosses_temp AS SELECT * FROM ProfitAndLosses;
                  
                  DROP TABLE ProfitAndLosses;
                  
                  CREATE TABLE ProfitAndLosses (
                    Id                  INTEGER NOT NULL CONSTRAINT PK_ProfitAndLosses PRIMARY KEY AUTOINCREMENT,
                    AverageBuyPrice     TEXT NOT NULL,
                    AverageSellPrice    TEXT NOT NULL,
                    Base                TEXT NULL,
                    CommissionPaid      TEXT NOT NULL,
                    DollarProfit        TEXT NOT NULL,
                    Profit              TEXT NOT NULL,
                    QuantityBought      TEXT NOT NULL,
                    QuantitySold        TEXT NOT NULL,
                    Terms               TEXT NULL,
                    RealisedProfit      TEXT NOT NULL DEFAULT 0.0
                  );

                  CREATE UNIQUE INDEX IX_ProfitAndLosses_Id ON ProfitAndLosses (Id);

                  INSERT INTO ProfitAndLosses 
                  (
                    Id,
                    AverageBuyPrice,
                    AverageSellPrice,
                    Base,
                    CommissionPaid,
                    DollarProfit,
                    Profit,
                    QuantityBought,
                    QuantitySold,
                    Terms,
                    RealisedProfit
                  )
                  SELECT Id,
                    AverageBuyPrice,
                    AverageSellPrice,
                    Base,
                    CommissionPaid,
                    ReportingProfit,
                    Profit,
                    QuantityBought,
                    QuantitySold,
                    Terms,
                    UnrealisedProfit
                  FROM ProfitAndLosses_temp;
                  
                  DROP TABLE ProfitAndLosses_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");


            // BalanceHistories: Migrate ReportingAmount column to DollarAmount column, drop ReportingAmount & ReportingCurrency columns

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE BalanceHistories_temp AS SELECT * FROM BalanceHistories;
                  
                  DROP TABLE BalanceHistories;
                  
                  CREATE TABLE BalanceHistories (
                    Key                 INTEGER NOT NULL CONSTRAINT PK_BalanceHistories PRIMARY KEY AUTOINCREMENT,
                    Balance             TEXT NOT NULL,
                    DateTime            TEXT NOT NULL,
                    DollarAmount        TEXT NOT NULL,
                    Name                TEXT NULL
                  );
                  
                  CREATE UNIQUE INDEX IX_BalanceHistories_Key ON BalanceHistories (Key);

                  INSERT INTO BalanceHistories 
                  (
                    Key,
                    Balance,
                    DateTime,
                    DollarAmount,
                    Name
                  )
                  SELECT Key,
                    Balance,
                    DateTime,
                    ReportingAmount,
                    Name
                  FROM BalanceHistories_temp;
                  
                  DROP TABLE BalanceHistories_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");


            // NOT MY CHANGES!!! Looks like model classes were updated without a db migration being created - this should bring the db up-to-date.

            migrationBuilder.Sql(
                @"PRAGMA foreign_keys = 0;
     
                  CREATE TABLE WalletBalances_temp AS SELECT * FROM WalletBalances;
                  
                  DROP TABLE WalletBalances;
                  
                  CREATE TABLE WalletBalances (
                    Id                  INTEGER NOT NULL CONSTRAINT PK_WalletBalances PRIMARY KEY AUTOINCREMENT,
                    Address             TEXT NOT NULL,
                    Available           TEXT NOT NULL,
                    Balance             TEXT NOT NULL,
                    BtcAmount           TEXT NOT NULL,
                    Currency            TEXT NULL,
                    Exchange            TEXT NULL,
                    Pending             TEXT NOT NULL,
                    PercentageChange    TEXT NOT NULL,
                    Price               TEXT NOT NULL,
                    Requested           TEXT NOT NULL,
                    Timestamp           TEXT NOT NULL,
                    Uuid                TEXT NULL
                  );
                  
                  CREATE UNIQUE INDEX IX_WalletBalances_Id ON WalletBalances (Id);

                  INSERT INTO WalletBalances 
                  (
                    Id,
                    Address,
                    Available,
                    Balance,
                    BtcAmount,
                    Currency,
                    Exchange,
                    Pending,
                    PercentageChange,
                    Price,
                    Requested,
                    Timestamp,
                    Uuid
                  )
                  SELECT Id,
                    '0.0',
                    Available,
                    Balance,
                    BtcAmount,
                    Currency,
                    Exchange,
                    Pending,
                    PercentageChange,
                    Price,
                    '0.0',
                    Timestamp,
                    Uuid
                  FROM WalletBalances_temp;
                  
                  DROP TABLE WalletBalances_temp;
                  
                  PRAGMA foreign_keys = 1;
                ");
        }
    }
}
