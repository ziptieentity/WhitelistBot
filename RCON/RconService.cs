using CoreRCON;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WhitelistBot.RCON;

public sealed class RconService(IConfiguration config, ILogger<RconService> logger)
{
    public async Task SendCommand(string command)
    {
        var ip = config.GetSection("RCON:IP").Get<string>();
        var port = config.GetSection("RCON:Port").Get<ushort>();
        var password = config.GetSection("RCON:Password").Get<string>();

        if (string.IsNullOrEmpty(ip) || port == 0 || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("RCON configuration is not properly set.");

        var addresses = await Dns.GetHostAddressesAsync(ip);
        var targetIp = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                       ?? addresses.FirstOrDefault()
                       ?? throw new InvalidOperationException("Could not resolve host to any valid IP address.");

        try
        {
            using var rcon = new CoreRCON.RCON(targetIp, port, password);
            await rcon.ConnectAsync();
            await rcon.SendCommandAsync(command);

            logger.LogInformation("Sent RCON command: {Command}", command);
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "Failed to connect to RCON server at {Host}:{Port}", ip, port);
            throw new InvalidOperationException($"RCON connection failed to {ip}:{port}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending RCON command: {Command}", command);
            throw;
        }
    }
}