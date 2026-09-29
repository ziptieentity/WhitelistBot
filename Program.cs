using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WhitelistBot.Bot;
using WhitelistBot.Data;
using WhitelistBot.RCON;

namespace WhitelistBot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateApplicationBuilder(args);
        host.Configuration.AddJsonFile("settings.json");
        host.Configuration.AddUserSecrets<Program>();
        host.Services.AddSingleton<DiscordControlMessage>();
        host.Services.AddSingleton<DiscordSocketClient>(p =>
            new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.Guilds
                }
            ));
        host.Services.AddSingleton<RconService>();
        host.Services.AddSingleton<WhitelistService>();
        host.Services.AddDbContextFactory<WhitelistDataContext>(x => { x.UseSqlite("Data Source=main.db"); });
        host.Services.AddHostedService<DiscordBot>();
        host.Services.AddSingleton(sp => new InteractionService(
            sp.GetRequiredService<DiscordSocketClient>()
        ));
        host.Services.AddTransient<BotInteractions>();
        using var app = host.Build();

        using (var scope = app.Services.CreateScope())
        {
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<WhitelistDataContext>>();
            await using var context = await contextFactory.CreateDbContextAsync();
            await context.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}