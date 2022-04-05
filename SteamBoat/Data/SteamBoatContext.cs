using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SteamBoat.Models;

namespace SteamBoat.Data
{
    public class SteamBoatContext : DbContext
    {
        public SteamBoatContext (DbContextOptions<SteamBoatContext> options)
            : base(options)
        {
        }
        public DbSet<SteamBoat.Models.Item> Items { get; set; }
        public DbSet<SteamBoat.Models.Mission> Mission { get; set; }

        public DbSet<SteamBoat.Models.FeederUrl> FeederUrl { get; set; }

        public DbSet<SteamBoat.Models.ItemForSale> ItemsForSale { get; set; }

    }
}

