using SignalRJWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace SignalRJWT.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IOptionsSnapshot<JwtSettings> jwtSettings;
        private readonly UserManager<MyUser> userManager;
        private readonly RoleManager<MyRole> roleManager;

        public TestController(IOptionsSnapshot<JwtSettings> jwtSettings, UserManager<MyUser> userManager,RoleManager<MyRole> roleManager)
        {
            this.jwtSettings = jwtSettings;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        [HttpPost]
        public async Task<ActionResult<string>> CreateDefault()
        {
            if (await roleManager.RoleExistsAsync("admin") == false)
            {
                MyRole role = new() { Name = "admin" };
                await roleManager.CreateAsync(role).CheckAsync();
            }

            MyUser? user1 = await userManager.FindByNameAsync("whid");
            if (user1 == null)
            {
                user1 = new MyUser() { UserName = "whid", Email = "wtf123@outlook.com", EmailConfirmed = true };
                await userManager.CreateAsync(user1, "123456").CheckAsync();
                await userManager.AddToRoleAsync(user1, "admin").CheckAsync();
            }
            return Ok("Ok");
        }
        
        [HttpPost]
        public async Task<ActionResult<string>> Login(UserInfo loginInfo)
        {
            MyUser? user = await userManager.FindByNameAsync(loginInfo.userName);
            if (user == null) return BadRequest("用户名或密码错误");
            if (await userManager.IsLockedOutAsync(user)) return BadRequest("用户已被锁定，锁定结束时间: " + user.LockoutEnd);
            if (await userManager.CheckPasswordAsync(user, loginInfo.password))
            {
                await userManager.ResetAccessFailedCountAsync(user).CheckAsync();

                user.JWTVersion++;
                await userManager.UpdateAsync(user).CheckAsync();

                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));

                claims.Add(new Claim(ClaimTypes.Version, user.JWTVersion.ToString()));

                var roles = await userManager.GetRolesAsync(user);
                foreach (string role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                string jwtToken = JwtHelper.BuildToken(claims, jwtSettings.Value);

                return Ok(new
                {
                    name = user.UserName,
                    token = jwtToken,
                });
            }
            else
            {
                await userManager.AccessFailedAsync(user);
                return BadRequest("用户名或密码错误");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUser(UserInfo user)
        {
            MyUser newUser = new MyUser() { UserName = user.userName };
            await userManager.CreateAsync(newUser, user.password).CheckAsync();
            return Ok();
        }
    }
    public record UserInfo(string userName, string password);
}
