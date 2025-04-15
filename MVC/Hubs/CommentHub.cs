using Microsoft.AspNetCore.SignalR;

namespace MVC.Hubs
{
    public class CommentHub : Hub
    {
        public async Task JoinProjectGroup(string projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }

        public async Task LeaveProjectGroup(string projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
        }

        public async Task NewComment(string projectId, object comment)
        {
            await Clients.Group($"project-{projectId}").SendAsync("ReceiveComment", comment);
        }

        public async Task DeletedComment(string projectId, string commentId)
        {
            await Clients.Group($"project-{projectId}").SendAsync("CommentDeleted", commentId);
        }
    }
}