using DiamondCDN.Bot.Common.Models;
using DiamondCDN.Bot.Services.Interfaces;
using Discord.Interactions;

namespace DiamondCDN.Bot.Commands;

[Group("tickets", "Commands related to the tickets system")]
[RequireContext(ContextType.Guild)]
public class Tickets : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ITicketService _ticketService;

    public Tickets(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [SlashCommand("new", "Creates a new ticket for the specified reason")]
    public async Task NewTicketAsync([Summary("type", "The type of query or issue you're experiencing")] TicketType type)
    {
        await _ticketService.CreateTicketAsync(Context.Interaction, type);
    }
}