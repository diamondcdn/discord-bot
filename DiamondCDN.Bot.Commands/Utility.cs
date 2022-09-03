using System.Reflection;
using System.Text;
using DiamondCDN.Bot.Common;
using DiamondCDN.Bot.Common.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using DiamondEmbedBuilder = DiamondCDN.Bot.Commands.Utils.DiamondEmbedBuilder;

namespace DiamondCDN.Bot.Commands;

public class Utility : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Random _rng = new();
    private readonly IConfiguration _configuration;

    public Utility(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [RequireContext(ContextType.Guild)]
    [SlashCommand("ping", "Returns the ping between the bot and the Discord gateway")]
    public async Task PingAsync()
    {
        await Context.Interaction.DeferAsync();

        var embed = new Common.Utils.DiamondEmbedBuilder()
            .WithTitle("Ping")
            .WithDescription($"The ping is {Context.Client.Latency}ms")
            .Build();

        await Context.Interaction.FollowupAsync(embed: embed);
    }

    [RequireContext(ContextType.Guild)]
    [UserCommand("Get avatar")]
    public async Task AvatarAsync(IUser user)
    {
        await Context.Interaction.DeferAsync(true);

        var avatarUrl = user.GetAvatarUrl();

        var embed = new Common.Utils.DiamondEmbedBuilder()
            .WithImageUrl(avatarUrl)
            .Build();

        await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
    }

    [RequireContext(ContextType.Guild)]
    [UserCommand("PP size")]
    public async Task GetPpSizeAsync(IUser user)
    {
        await Context.Interaction.DeferAsync();

        var ppSize = _rng.Next(0, 20);
        var ppBars = string.Empty;
        for (var i = 0; i < ppSize; i++) ppBars += "=";

        var embed = new Common.Utils.DiamondEmbedBuilder()
            .WithTitle($"{user.Username}'s PP")
            .WithDescription($"8{ppBars}D")
            .Build();

        await Context.Interaction.FollowupAsync(embed: embed);
    }

    [RequireContext(ContextType.Guild)]
    [UserCommand("Get user information")]
    public async Task GetUserInformationAsync(IUser user)
    {
        await Context.Interaction.DeferAsync(true);

        var guildUser = user as SocketGuildUser;

        var clients = user.ActiveClients?
            .Aggregate(string.Empty, (current, clientType) => current + $"{clientType}, ")
            .TrimEnd(' ').TrimEnd(',');

        var roles = guildUser?.Roles?.OrderByDescending(x => x.Position)
            .Aggregate(string.Empty, (current, role) => current + $"{role.Mention} ");

        var embed = new Common.Utils.DiamondEmbedBuilder()
            .WithThumbnailUrl(user.GetAvatarUrl())
            .AddField("> Username", user.Username)
            .AddField("> Nickname", guildUser?.Nickname ?? "None")
            .AddField("> Discriminator", user.Discriminator)
            .AddField("> Status", user.Status)
            .AddField("> Created at", user.CreatedAt)
            .AddField("> Roles", string.IsNullOrWhiteSpace(roles) ? "None" : roles)
            .AddField("> Active clients", string.IsNullOrWhiteSpace(clients) ? "None" : clients)
            .AddField("> Joined at", guildUser.JoinedAt)
            .Build();

        await Context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
    }

    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [SlashCommand("clean", "Cleans up the bots responses")]
    public async Task CleanAsync()
    {
        await Context.Interaction.DeferAsync(true);

        var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
        var safeMessages = messages.Where(x => x.Author.Id == Context.Client.CurrentUser.Id);

        if (!safeMessages.Any())
        {
            await Context.Interaction.ErrorAsync("Cannot delete messages", "There is nothing for me to delete", true);
            return;
        }

        await ((SocketTextChannel) Context.Channel).DeleteMessagesAsync(safeMessages);

        await Context.Interaction.SuccessAsync("Responses cleaned", $"**{safeMessages.Count()}** response(s) have been deleted.", true);
    }

    [RequireContext(ContextType.Guild)]
    [MessageCommand("Reverse message")]
    public async Task ReverseMessage(IMessage message)
    {
        await Context.Interaction.DeferAsync(true);

        var reversedMessageChars = message.Content.Reverse();
        var reversedMessage = reversedMessageChars.Aggregate(string.Empty, (current, item) => current + item);

        await Context.Interaction.FollowupAsync(reversedMessage, ephemeral: true);
    }

    [RequireContext(ContextType.Guild)]
    [MessageCommand("Base64 decode")]
    public async Task Base64Decode(IMessage message)
    {
        await Context.Interaction.DeferAsync(true);

        try
        {
            var content = Convert.FromBase64String(message.Content);

            await Context.Interaction.FollowupAsync(Encoding.UTF8.GetString(content), ephemeral: true);
        }
        catch (Exception)
        {
            await Context.Interaction.ErrorAsync("Cannot convert message", "The message is not a Base64 string.", true);
        }
    }

    [RequireContext(ContextType.Guild)]
    [MessageCommand("Base64 encode")]
    public async Task Base64Encode(IMessage message)
    {
        await Context.Interaction.DeferAsync(true);

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(message.Content));

        await Context.Interaction.FollowupAsync(base64, ephemeral: true);
    }

    [RequireContext(ContextType.Guild)]
    [SlashCommand("info", "Returns certain information about the bot")]
    public async Task InfoAsync()
    {
        await Context.Interaction.DeferAsync();

        var embed = new DiamondEmbedBuilder()
            .WithTitle("About the bot")
            .AddField("> Version", $"DiamondCDN.Bot v{Assembly.GetEntryAssembly()?.GetName().Version}")
            .AddField("> Repository", "https://github.com/diamondcdn/discord-bot")
            .AddField("> Based off", "https://github.com/AuthwareCloud/discord-bot")
            .AddField("> OS", Environment.OSVersion.VersionString);

        await Context.Interaction.FollowupAsync(embed: embed.Build());
    }
}