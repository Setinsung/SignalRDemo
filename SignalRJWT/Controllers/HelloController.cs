using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SignalRJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult Hello()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            string userName = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            IEnumerable<Claim> roleClaims = User.FindAll(ClaimTypes.Role);
            string roleNames = string.Join(',', roleClaims.Select(c => c.Value));
            return Ok($"id={id},userName={userName},roleNames ={roleNames}");
        }
    }
}
