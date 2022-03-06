﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public DbSet<SteamBoat.Models.Mission> Mission { get; set; }
    }
}