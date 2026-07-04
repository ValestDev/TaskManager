using Microsoft.AspNetCore.SignalR;
using TaskManager.API.Hubs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Realtime;

public class SignalRPresenceNotifier : IPresenceNotifier
{
    private readonly IHubContext<PresenceHub> _hubContext;

    public SignalRPresenceNotifier(IHubContext<PresenceHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserOnline(Guid userId, string userName, DateTime loginAt)
    {
        await _hubContext.Clients.All.SendAsync("UserOnline", new
        {
            userId,
            userName,
            loginAt
        });
    }

    public async Task NotifyUserOffline(Guid userId, string userName)
    {
        await _hubContext.Clients.All.SendAsync("UserOffline", new
        {
            userId,
            userName
        });
    }
}