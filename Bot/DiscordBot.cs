using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WhitelistBot.Data;

namespace WhitelistBot.Bot;

public sealed class DiscordBot(
    DiscordSocketClient client,
    DiscordControlMessage controlMessage,
    InteractionService interactionService,
    IServiceProvider services,
    ILogger<DiscordBot> logger,
    IConfiguration config)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = config.GetSection("Discord:Token").Get<string>();
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Discord token is not configured.");

        client.Log += OnLog;
        client.Ready += Ready;
        client.InteractionCreated += HandleInteraction;
        await interactionService.AddModulesAsync(
            assembly: Assembly.GetEntryAssembly()!,
            services: services);
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    private async Task Ready()
    {
        try
        {
            var commands = await interactionService.RegisterCommandsGloballyAsync();
            await EnsureControlMessage();
            logger.LogInformation("Bot ready on user: {UserId}.", client.CurrentUser.Id);
            logger.LogInformation("Successfully registered {CommandCount} commands globally.", commands.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add interaction modules.");
            return;
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(client, interaction);
        await interactionService.ExecuteCommandAsync(context, services);
    }

    private Task OnLog(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Verbose:
                logger.LogTrace(message.ToString());
                break;
            case LogSeverity.Debug:
                logger.LogDebug(message.ToString());
                break;
            case LogSeverity.Info:
                logger.LogInformation(message.ToString());
                break;
            case LogSeverity.Warning:
                logger.LogWarning(message.ToString());
                break;
            case LogSeverity.Error:
                logger.LogError(message.ToString());
                break;
            case LogSeverity.Critical:
                logger.LogCritical(message.ToString());
                break;
            default:
                logger.LogInformation(message.ToString());
                break;
        }

        return Task.CompletedTask;
    }

    private async Task EnsureControlMessage()
    {
        var whitelistChannelId = config.GetSection("Discord:WhitelistChannel").Get<ulong>();
        if (await client.GetChannelAsync(whitelistChannelId) is not ITextChannel channel)
            throw new InvalidOperationException("Discord whitelist channel is not found.");

        var controlMessageId = controlMessage.MessageId;
        if (controlMessageId == 0)
        {
            await SendNewControlMessage(channel);
            return;
        }

        var message = await channel.GetMessageAsync(controlMessageId);
        if (message is not IUserMessage userMessage)
        {
            await SendNewControlMessage(channel);
            return;
        }

        await userMessage.ModifyAsync(msg =>
        {
            msg.Embed = controlMessage.Embed.Build();
            msg.Components = controlMessage.Components.Build();
        });
    }

    private async Task SendNewControlMessage(ITextChannel channel)
    {
        var newMessage = await channel.SendMessageAsync(embed: controlMessage.Embed.Build(),
            components: controlMessage.Components.Build());
        controlMessage.SetControlMessage(newMessage.Id);
    }
}