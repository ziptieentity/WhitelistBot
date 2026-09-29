using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WhitelistBot.Data;

namespace WhitelistBot.Bot;

public sealed class BotInteractions(ILogger<BotInteractions> logger, WhitelistService whitelistService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [ComponentInteraction("whitelist_me")]
    public async Task OnWhitelist()
    {
        await RespondWithModalAsync<WhitelistModal>("whitelist_modal");
    }

    [ComponentInteraction("refresh_whitelist")]
    public async Task OnWhitelistRefresh()
    {
        try
        {
            logger.LogInformation("Refreshing whitelist for {User}", Context.User.Username);
            await whitelistService.RefreshWhitelist(Context.User.Id);
            await RespondAsync("Successfully refreshed whitelist.", ephemeral: true);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "An error occurred while refreshing whitelist for {User}", Context.User.Username);
        }
    }

    [ComponentInteraction("unwhitelist_me")]
    public async Task OnUnwhitelist()
    {
        try
        {
            logger.LogInformation("Unwhitelist for {User}", Context.User.Username);
            await whitelistService.Unwhitelist(Context.User.Id);
            await RespondAsync("Successfully unwhitelisted.", ephemeral: true);
        }
        catch (Exception ex)
        {
            await RespondAsync("An error occurred whilst unwhitelisting: " + ex.Message, ephemeral: true);
        }
    }

    [ModalInteraction("whitelist_modal")]
    public async Task HandleWhitelistModal(WhitelistModal modal)
    {
        try
        {
            logger.LogInformation("Whitelist for {User} with MC username: {MC}", Context.User.Username,
                modal.MinecraftUsername);
            await whitelistService.Whitelist(modal.MinecraftUsername, Context.User.Id);
            await RespondAsync("Successfully whitelisted: " + modal.MinecraftUsername, ephemeral: true);
        }
        catch (Exception ex)
        {
            await RespondAsync("An error occurred whilst whitelisting: " + ex.Message, ephemeral: true);
        }
    }
}