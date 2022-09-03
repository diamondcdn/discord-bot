using Discord;

namespace DiamondCDN.Bot.Commands.Utils;

public class DiamondEmbedBuilder : EmbedBuilder
{
    public DiamondEmbedBuilder()
    {
        WithFooter("diamondcdn.com");
        WithCurrentTimestamp();
        WithColor(new Color(67, 56, 202));
    }
}