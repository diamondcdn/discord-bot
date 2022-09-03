using DiamondCDN.Bot.Commands;
using DiamondCDN.Bot.Common;
using DiamondCDN.Bot.Services;
using DiamondCDN.Bot.Services.Interfaces;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DiamondCDN.Bot;

public class Startup
{
    private readonly InteractionService _interaction;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _provider;

    private IConfiguration Configuration { get; }

    private string[] Arguments { get; }

    public Startup(string[] args, InteractionService? interaction = null, DiscordSocketClient? client = null)
    {
        Arguments = args;

        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        _client = client ?? new DiscordSocketClient(new DiscordSocketConfig
        {
            DefaultRetryMode = RetryMode.RetryRatelimit,
            LogLevel = LogSeverity.Debug
        });

        _interaction = interaction ?? new InteractionService(_client, new InteractionServiceConfig
        {
            UseCompiledLambda = true,
            EnableAutocompleteHandlers = true,
            ExitOnMissingModalField = true,
            DefaultRunMode = RunMode.Async
        });

        // This stops commands from not getting registered.
        _provider = BuildServiceProvider();
        Globals.ServiceProvider = _provider;

        _interaction.AddModulesAsync(typeof(CommandsEntryPoint).Assembly, _provider);

        _client.Ready += HandleOnReady;
    }

    private async Task HandleOnReady()
    {
#if DEBUG
        _provider.GetRequiredService<ILogger<Startup>>().LogInformation("Registering slash commands to guild...");
        var registeredCommands =
            await _interaction.RegisterCommandsToGuildAsync(Configuration.GetValue<ulong>("TestingGuildId"));
        foreach (var command in registeredCommands)
            _provider.GetRequiredService<ILogger<Startup>>().LogInformation("Registered {Name}", command.Name);
#else
        _provider.GetRequiredService<ILogger<Startup>>().LogInformation("Registering slash commands globally...");
        await _interaction.RegisterCommandsGloballyAsync();
#endif

        await _client.SetActivityAsync(new Game("your websites", ActivityType.Watching));
        
        await _provider.GetRequiredService<ITicketService>().SendSupportMessageAsync();
    }

    public async Task RunAsync()
    {
        _provider.GetRequiredService<LoggingService>();
        _provider.GetRequiredService<InteractionHandlerService>();
        await _provider.GetRequiredService<IStartupService>().StartAsync();

        await Task.Delay(-1);
    }

    private IServiceProvider BuildServiceProvider()
    {
        return new ServiceCollection()
            .AddLogging(options =>
            {
#if DEBUG
                options.SetMinimumLevel(LogLevel.Debug);
                options.AddConsole();
#else
                options.SetMinimumLevel(LogLevel.Information);
#endif
            })
            .AddSingleton(_client)
            .AddSingleton(_interaction)
            .AddSingleton(Configuration)
            .AddSingleton<LoggingService>()
            .AddSingleton<InteractionHandlerService>()
            .AddSingleton<ITicketService, TicketService>()
            .AddSingleton<IStartupService, StartupService>()
            .BuildServiceProvider();
    }
}