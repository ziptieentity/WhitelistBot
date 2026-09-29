using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WhitelistBot.Data;

public sealed class WhitelistDataContext : DbContext
{
    public WhitelistDataContext(DbContextOptions<WhitelistDataContext> options) : base(options)
    {
    }

    public DbSet<WhitelistEntry> WhitelistEntries { get; set; } = null!;

    public class WhitelistEntry
    {
        [Key] public ulong DiscordUserId { get; set; }
        public string MinecraftUsername { get; set; }
    }
}