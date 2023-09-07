using Microsoft.AspNetCore.Identity;

namespace SignalRJWT.Models
{
    public class MyUser: IdentityUser<long>
    {
        public long JWTVersion { get; set; }    
    }
}
