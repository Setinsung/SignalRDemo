using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRJWT.Hubs;
using SignalRJWT.Models;

namespace SignalRJWT.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SignalRDemoController : ControllerBase
    {
        private readonly IHubContext<ChatRoomHub> hubContext;
        private readonly UserManager<MyUser> userManager;

        public SignalRDemoController(IHubContext<ChatRoomHub> hubContext, UserManager<MyUser> userManager)
        {
            this.hubContext = hubContext;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<string>> AddUser(UserInfo userInfo)
        {
            MyUser user1 = new MyUser() { UserName = userInfo.userName };
            await userManager.CreateAsync(user1, userInfo.password).CheckAsync();
            await hubContext.Clients.All.SendAsync("ReceivePublicMessage", $"欢迎{userInfo.userName}的加入");
            return Ok("添加成功");
        }
    }
}
