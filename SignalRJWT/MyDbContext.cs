using SignalRJWT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SignalRJWT
{
    public class MyDbContext : IdentityDbContext<MyUser, MyRole, long>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options): base(options)
        {
        }
    }
}
