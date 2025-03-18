using Microsoft.AspNetCore.SignalR;

namespace InventoryManagement.Infrastructure.Hubs
{
    public class InventoryHub : Hub
    {
        public async Task SendUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveUpdate", message);
        }
    }
}
