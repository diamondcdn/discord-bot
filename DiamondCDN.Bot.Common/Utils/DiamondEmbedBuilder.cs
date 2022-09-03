using Discord;

namespace DiamondCDN.Bot.Common.Utils;

public class DiamondEmbedBuilder : EmbedBuilder
{
    public DiamondEmbedBuilder()
    {
        WithFooter("diamondcdn.com");
        WithCurrentTimestamp();
        WithColor(new Color(67, 56, 202));
    }
}