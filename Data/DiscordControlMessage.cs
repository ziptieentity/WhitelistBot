using Discord;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhitelistBot.Data;

public sealed class DiscordControlMessage(IConfiguration config)
{
    public ulong MessageId => _messageId ?? LoadControlMessage();

    public EmbedBuilder Embed { get; } = new EmbedBuilder()
        .WithTitle($"{config["Minecraft:Name"]} Whitelist")
        .WithDescription(
            "Click the buttons below to manage your whitelist on the MC server.")
        .WithFields().AddField("IP", "ip.ziptieentity.com")
        .WithFields().AddField("Version", config["Minecraft:Version"] ?? "Unknown")
        .WithColor(Color.Blue);

    public ComponentBuilder Components { get; } = new ComponentBuilder()
        .WithButton("Whitelist Me", "whitelist_me")
        .WithButton("Refresh Whitelist", "refresh_whitelist", ButtonStyle.Secondary)
        .WithButton("Unwhitelist Me", "unwhitelist_me", ButtonStyle.Danger);

    private ulong? _messageId;

    private ulong LoadControlMessage()
    {
        if (!File.Exists("cm"))
            return 0;
        var data = File.ReadAllText("cm");
        if (!ulong.TryParse(data, out var messageId))
            return 0;
        _messageId = messageId;
        return messageId;
    }

    public void SetControlMessage(ulong messageId)
    {
        _messageId = messageId;
        File.WriteAllText("cm", messageId.ToString());
    }
}