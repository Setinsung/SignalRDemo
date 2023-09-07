using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace SignalRJWT
{
    public static class IdentityHelper
    {
        public static async Task CheckAsync(this Task<IdentityResult> task) // 检测identityResult是否Succeeded == true，失败抛出异常
        {
            IdentityResult identityResult = await task;
            if(!identityResult.Succeeded)
            {
                throw new Exception(JsonSerializer.Serialize(identityResult.Errors));
            }
        }
    }
}
