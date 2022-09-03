using DiamondCDN.Bot.Common;
using DiamondCDN.Bot.Common.Models;
using DiamondCDN.Bot.Common.Utils;
using DiamondCDN.Bot.Services.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiamondCDN.Bot.Services;

public class TicketService : ITicketService
{
    #region Services

    private readonly ILogger<TicketService> _logger;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;

    #endregion

    public TicketService(ILogger<TicketService> logger, IConfiguration configuration, DiscordSocketClient client)
    {
        _logger = logger;
        _configuration = configuration;
        _client = client;
    }

    public async Task CreateTicketAsync(SocketInteraction interaction, TicketType type)
    {
        if (!_configuration.IsHomeGuild(interaction.GuildId ?? 0))
        {
            _logger.LogInformation("Unable to create ticket due to home guild mismatch");
            return;
        }

        _logger.LogInformation("Creating a ticket for {User}", interaction.User.Username);

        await interaction.DeferAsync(true);
        var guild = _client.GetGuild(interaction.GuildId ?? 0);

        if (guild.Channels.Any(x => x.Name == $"ticket-{interaction.User.Id}"))
        {
            _logger.LogInformation("Unable to create ticket due to ticket already open by another user");

            await interaction.ErrorAsync("Cannot create ticket",
                "You already have a ticket open, close the existing one before opening another!", true);
            return;
        }

        // Get the support role (the users authorized to see the ticket) or create it if non existent

        var authorizedSupportRole = guild.Roles.FirstOrDefault(x =>
                                        x.Name.Equals(_configuration.GetValue<string>("SupportRole"),
                                            StringComparison.OrdinalIgnoreCase)) as IRole ??
                                    await guild.CreateRoleAsync(_configuration.GetValue<string>("SupportRole"),
                                        GuildPermissions.None, Color.Default,
                                        false, true);

        var ticketCategory = guild.CategoryChannels.FirstOrDefault(x =>
                                 x.Name.Equals("tickets", StringComparison.OrdinalIgnoreCase)) ??
                             (ICategoryChannel?)await guild.CreateCategoryChannelAsync("Tickets",
                                 properties =>
                                 {
                                     properties.Position = guild.Channels.MaxBy(x => x.Position).Position + 1;
                                 });

        // Create the actual ticket channel
        var ticketChannel = await guild.CreateTextChannelAsync($"ticket-{interaction.User.Id}", properties =>
        {
            properties.Topic =
                $"Support ticket for {interaction.User.Username}#{interaction.User.Discriminator} regarding an issue relating to {type.ToString().ToLower()}";
            if (ticketCategory is not null) properties.CategoryId = ticketCategory.Id;
        });

        await ticketChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
            OverwritePermissions.DenyAll(ticketChannel));
        await ticketChannel.AddPermissionOverwriteAsync(authorizedSupportRole,
            OverwritePermissions.AllowAll(ticketChannel));
        await ticketChannel.AddPermissionOverwriteAsync(interaction.User, OverwritePermissions.AllowAll(ticketChannel));

        var ticketEmbed = new DiamondEmbedBuilder()
            .WithTitle($"Welcome, {interaction.User.Username}!")
            .WithDescription("A support representative should be with you shortly, please be patient!")
            .AddField("> Topic", type)
            .Build();

        var ticketComponent = new ComponentBuilder()
            .WithButton("Close", $"close-ticket-{interaction.User.Id}", ButtonStyle.Danger)
            .Build();

        var message = await ticketChannel.SendMessageAsync(
            $"{interaction.User.Mention} {authorizedSupportRole.Mention}",
            embed: ticketEmbed, components: ticketComponent);

        await message.PinAsync();

        await interaction.SuccessAsync("Ticket created",
            $"Your ticket has been created, check out {ticketChannel.Mention}!", true);
    }

    private static IEnumerable<SelectMenuOptionBuilder> YieldOptions()
    {
        var values = Enum.GetValues<TicketType>();
        foreach (var val in values)
            yield return new SelectMenuOptionBuilder()
                .WithLabel(val.ToString())
                .WithValue(val.ToString());
    }

    public async Task SendSupportMessageAsync()
    {
        var channelId = _configuration.GetValue<ulong>("SupportChannelId");
        if (await _client.GetChannelAsync(channelId) is not SocketTextChannel channel)
        {
            _logger.LogWarning(
                "Unable to fetch support message channel, ensure that the 'SupportChannelId' is a valid ulong value");
            return;
        }

        var messages = await channel.GetMessagesAsync().FlattenAsync();
        
        if (messages.All(x => x.Author.Id != _client.CurrentUser.Id))
        {
            var embed = new DiamondEmbedBuilder()
                .WithTitle("Open a ticket")
                .WithDescription(
                    "Experiencing an issue, have a inquiry or just want to talk one-to-one with one of our staff members, you can open a ticket below");

            var options = YieldOptions();

            var ticketComponent = new ComponentBuilder()
                .WithSelectMenu("open-ticket", options.ToList())
                .Build();

            await channel.SendMessageAsync(embed: embed.Build(), components: ticketComponent);
        }
    }
}