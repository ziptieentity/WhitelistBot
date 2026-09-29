using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WhitelistBot.RCON;

namespace WhitelistBot.Data;

public sealed class WhitelistService(IDbContextFactory<WhitelistDataContext> contextFactory, RconService rconService)
{
    public async Task Whitelist(string username, ulong discordUserId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        if (context.WhitelistEntries.Any(x => x.DiscordUserId == discordUserId))
        {
            throw new InvalidOperationException("User is already whitelisted");
        }

        var entry = new WhitelistDataContext.WhitelistEntry()
        {
            MinecraftUsername = username,
            DiscordUserId = discordUserId
        };
        context.WhitelistEntries.Add(entry);
        await context.SaveChangesAsync();
        await RefreshWhitelist(discordUserId, username);
    }

    public async Task Unwhitelist(ulong discordUserId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var entry = await context.WhitelistEntries.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
        if (entry is null)
        {
            throw new InvalidOperationException("User is not whitelisted");
        }

        context.WhitelistEntries.Remove(entry);
        await context.SaveChangesAsync();
        await rconService.SendCommand("whitelist remove " + entry.MinecraftUsername);
    }

    public async Task RefreshWhitelist(ulong discordUserId, string? username = null)
    {
        if (username == null)
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var entry = await context.WhitelistEntries.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
            if (entry is null)
            {
                throw new InvalidOperationException("User is not whitelisted");
            }

            username = entry.MinecraftUsername;
        }

        await rconService.SendCommand("whitelist add " + username);
    }
}