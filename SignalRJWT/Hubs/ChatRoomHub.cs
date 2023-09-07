using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignalRJWT.Hubs
{
    [Authorize]
    public class ChatRoomHub : Hub
    {
        // [Authorize]
        public Task SendPublicMessage(string msg)
        {
            var claim = Context.User.FindFirst(ClaimTypes.Name);
            string connId = Context.ConnectionId;
            string msgToSend = $"Name:{claim.Value} Id:{connId} Time:{DateTime.Now} Msg:{msg}";
            return Clients.All.SendAsync("ReceivePublicMessage", msgToSend);
        }
    }
}
