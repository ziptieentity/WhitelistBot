using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhitelistBot.Bot;

public sealed class WhitelistModal : IModal
{
    public string Title { get; } = "Cybergoons MC Whitelist";

    [ModalTextInput("minecraft_username"), InputLabel("Minecraft Username", "Your Minecraft username."), RequiredInput]
    public string MinecraftUsername { get; set; }
}