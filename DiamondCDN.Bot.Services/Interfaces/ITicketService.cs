using DiamondCDN.Bot.Common.Models;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiamondCDN.Bot.Services.Interfaces;

public interface ITicketService
{ 
    Task CreateTicketAsync(SocketInteraction interaction, TicketType type);
    Task SendSupportMessageAsync();
}