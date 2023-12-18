using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs
{
    public class SubscriptionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public async Task SendSubscriptionMessage(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveSubscriptionMessage", message);
        }
    }
}
