# TeleCoinigy

A simple telegram bot that sends your balance updates from coinigy

Will add releases soon.

Installation:

Pre-requisites: Net Core SDK: https://www.microsoft.com/net/download/core
Get your Bot ID and Chat ID. See https://github.com/LibreLabUCM/teleg-api-bot/wiki/Getting-started-with-the-Telegram-Bot-API
Fill in your config in appsettings.json. Bot ID is WITHOUT Bot
Create a folder called logs and another called database in the folder you have the dll. 
Start on command line with "dotnet TeleCoinigy.dll"

Usage:

Commands are: 
/acc {number of account from /all} - balance for specific account
/all - all account names
/total - total balance
