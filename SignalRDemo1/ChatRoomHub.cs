using Microsoft.AspNetCore.SignalR;

namespace SignalRDemo1
{
    public class ChatRoomHub : Hub
    {
        // 虽然方法是异步方法，但是是客户端通过方法名调用，所以可不用Async结尾
        public Task SendPublicMessage(string msg)
        {
            string connId = this.Context.ConnectionId;
            string msgToSend = $"{connId}-{DateTime.Now}: {msg}";
            return Clients.All.SendAsync("ReceivePublicMessage", msgToSend); // 只有一个异步调用直接return
        }
    }
}
