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
/help when the bot is running

**Done:**
* Coinigy balance notification
* Coinigy 24 hour PnL
* Bittrex Trade notifications
* Poloniex Trade notifications
* Bittrex csv order export upload
* Excel trade history export
* Price drop notifications when a balance drops more than 30%. Used for bag management. Runs 4 times a day. 
* Pair profit - TODO - Calculations are wrong. 

**Todo**
* Will only work if you have a poloniex, bittrex and coinigy account configured. Add functionality to have a combination of the above. 
* Multiple exchange accounts. 
* Exception handling

**Screenshots**


![Screenshot 1](https://github.com/mehtadone/CryptoGramBot/blob/master/CryptoGramBot/images/screenshot.png?raw=true)


