using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalRJWT
{
    public static class JwtHelper
    {
        public static string BuildToken(List<Claim> claims, JwtSettings settings)
        {
            DateTime expire = DateTime.Now.AddSeconds(settings.ExpireSeconds);
            SymmetricSecurityKey secKey = new(Encoding.UTF8.GetBytes(settings.SecKey));
            SigningCredentials credentials = new(secKey, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken token = new(
                claims: claims,
                expires: expire,
                signingCredentials: credentials
            );
            string jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
