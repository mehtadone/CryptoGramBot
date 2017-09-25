using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CryptoGramBot.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(CryptoGramBotDbContext context)
        {
            await context.Database.MigrateAsync();

            // Add Seed Data...
        }
    }
}