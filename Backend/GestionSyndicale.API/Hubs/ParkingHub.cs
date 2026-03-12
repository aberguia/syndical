using Microsoft.AspNetCore.SignalR;

namespace GestionSyndicale.API.Hubs;

public class ParkingHub : Hub
{
    public async Task JoinParkingGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Parking");
    }

    public async Task LeaveParkingGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Parking");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Parking");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Parking");
        await base.OnDisconnectedAsync(exception);
    }
}
