using Microsoft.AspNetCore.SignalR;

namespace MVC.Hubs;

public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
    }
    public async Task JoinProjectManagerGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "project-managers");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
    }

    public async Task LeaveProjectManagerGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "project-managers");
    }
}