using DiamondCDN.Bot.Common.Models;
using DiamondCDN.Bot.Common.Utils;
using DiamondCDN.Bot.Services.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiamondCDN.Bot.Services;

public class StartupService : IStartupService
{
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<StartupService> _logger;

    public StartupService(DiscordSocketClient client,
        IConfiguration configuration, ILogger<StartupService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync()
    {
        if (string.IsNullOrWhiteSpace(_configuration["Token"]))
            throw new ArgumentNullException(nameof(_configuration),
                "Bot token is null");

        _logger.LogInformation("Logging into bot account");

        await _client.LoginAsync(TokenType.Bot, _configuration["Token"]);
        await _client.StartAsync();
    }
}