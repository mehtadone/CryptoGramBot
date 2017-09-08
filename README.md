# CryptoGramBot


A simple telegram bot that sends your balance updates from coinigy, send trade notifications from Poloniex and Bittrex and creates you a trade export for your own spreadsheet magicary.


**Installation:**


* Pre-requisites: Net Core SDK: https://www.microsoft.com/net/download/core
* Get your Bot ID and Chat ID. See https://github.com/LibreLabUCM/teleg-api-bot/wiki/Getting-started-with-the-Telegram-Bot-API
* Fill in your config in appsettings.json. Bot ID is WITHOUT Bot
* Create a folder called logs and another called database in the folder you have the dll.
* Give CryptoGramBot the correct execute permissions via chmod
* Start on command line with "dotnet CryptoGramBot.dll"


**Usage:**
* Add keys and whether you want enable each service (true or false)
* Type /help when the bot is running

**Done:**
* Use a combination of bittrex, poloniex and/or Coinigy
* Coinigy 24 hour PnL with profit and loss in BTC and USD
* Bittrex Trade notifications with % profit if a sell
* Poloniex Trade notifications with % profit if a sell
* Bittrex csv order export upload
* Excel trade history export
* Price drop notifications when a balance drops more than 30%. Used for bag management. Runs 4 times a day.
* Pair profit - TODO - Calculations are wrong.

**Todo**
* Multiple exchange accounts.
* Exception handling
* Bittrex and poloniex balance info

**Screenshots**


![Screenshot 1](https://github.com/mehtadone/CryptoGramBot/blob/master/CryptoGramBot/images/screenshot.png?raw=true)
