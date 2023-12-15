using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Connected User: {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }

        public async Task SendTagNotification(string taggedUser, string message)
        {
            try
            {
                _logger.LogInformation($"Sending notification to user: {taggedUser}, Message: {message}");
                await Clients.User(taggedUser).SendAsync("ReceiveTagNotification", message);
                _logger.LogInformation("Notification sent successfully.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error sending notification: {ex.Message}");
            }
        }
    }
}

