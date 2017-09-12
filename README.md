# CryptoGramBot

[![Build status](https://ci.appveyor.com/api/projects/status/64877qbjrmvirbar/branch/master?svg=true)](https://ci.appveyor.com/project/mehtadone/telecoinigy/branch/master)

A simple telegram bot that sends your balance updates from coinigy, send trade notifications from Poloniex and Bittrex and creates you a trade export for your own spreadsheet magicary.

**Donations Welcome:**
* BTC: 1LVtLb6Vo79nyPBp252GSJVDMPToGvjFN6
* DASH: 0x20A660DB0Abb84f62c532E5881C90e0Ef0e29638
* ETH: LYGuFsyHSYFpmEiW4SKPedt6KsvL2ZqeEW
* LTC: XoQepSjoTEriBV7bLo1bdTVjbdy1AJW11B

**Installation:**

* Pre-requisites: Net Core SDK: https://www.microsoft.com/net/download/core
* Get your Bot ID and Chat ID. See https://github.com/LibreLabUCM/teleg-api-bot/wiki/Getting-started-with-the-Telegram-Bot-API
* Download the lastest version of the zip from https://github.com/mehtadone/CryptoGramBot/releases and unzip to a folder
* Fill in your config in appsettings.json. Bot ID is WITHOUT Bot and choose whether you want enable each service (true or false)
* Create a folder called logs and another called database in the folder you have the dll.
* Give CryptoGramBot the correct execute permissions via chmod if on linux
* Start on command line with "dotnet CryptoGramBot.dll"

**Usage:**
* Type /help when the bot is running

**Done:**
* Use a combination of bittrex, poloniex and/or Coinigy
* Coinigy 24 hour PnL with profit and loss in BTC and USD
* Bittrex Trade notifications with % profit if a sell 
* Poloniex Trade notifications with % profit if a sell
* Bittrex balance information
* Bittrex wallet information and % change since bought.
* Bittrex csv order export upload
* Excel trade history export
* Price drop notifications when a balance drops more than 30%. Used for bag management. Runs 4 times a day.
* Pair profit 

**Todo**
* Exception handling
* Poloniex balance info
* Show deposits and withdrawals to show accurate profit and loss. 
* Profit calculations are wrong on a sell if we don't have the data in the database

**Screenshots**

![Screenshot 1](https://github.com/mehtadone/CryptoGramBot/blob/master/CryptoGramBot/images/screenshot.png?raw=true)
![Screenshot 1](https://github.com/mehtadone/CryptoGramBot/blob/master/CryptoGramBot/images/screenshot-1.png?raw=true)
