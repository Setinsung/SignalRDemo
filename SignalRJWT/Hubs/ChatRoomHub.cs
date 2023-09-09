using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignalRJWT.Models;
using System.Security.Claims;

namespace SignalRJWT.Hubs
{
    [Authorize]
    public class ChatRoomHub : Hub
    {
        private readonly UserManager<MyUser> userManager;
        private readonly ImportExecutor executor;

        public ChatRoomHub(UserManager<MyUser> userManager, ImportExecutor executor)
        {
            this.userManager = userManager;
            this.executor = executor;
        }
        public Task SendPublicMessage(string msg)
        {
            var claim = Context.User.FindFirst(ClaimTypes.Name);
            string connId = Context.ConnectionId;
            string msgToSend = $"Name:{claim.Value} Id:{connId} Time:{DateTime.Now} Msg:{msg}";
            //await this.Groups.AddToGroupAsync(this.Context.ConnectionId, "dev");
            return Clients.All.SendAsync("ReceivePublicMessage", msgToSend);
        }

        public async Task<string> SendPrivateMessage(string toUserName, string msg)
        {
            MyUser? toUser = await userManager.FindByNameAsync(toUserName);
            if (toUser == null) return "查无此人";
            string currentUserName = this.Context.User.FindFirstValue(ClaimTypes.Name);
            string currentUserId = this.Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //await this.Clients.User(toUser.Id.ToString()).SendAsync("ReceicePrivateMessage", currentUserName, DateTime.Now.ToShortTimeString(), msg);
            var userList = new List<string> { currentUserId, toUser.Id.ToString() };
            await this.Clients.Users(userList).SendAsync("ReceicePrivateMessage", currentUserName, toUserName, DateTime.Now.ToShortTimeString(), msg);
            return "发送成功";
        }

        public Task ImportECDict()
        {
            _ = executor.ExecuteAsync(this.Context.ConnectionId);
            //_ = Task.Run(async () =>
            //{
            //    await executor.ExecuteAsync(this.Context.ConnectionId);
            //});
            return Task.CompletedTask;
        }

    }
}
