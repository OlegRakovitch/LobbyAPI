using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace RattusAPI.Authentication
{
    public class TrustedAuthenticationProvider : IAuthenticationProvider
    {
        public string RegisteredName => "Trusted";

        public ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context)
        {
            var header = context.HttpContext.Request.Headers["username"];
            if (header == StringValues.Empty)
            {
                return null;
            }
            else
            {
                var username = header.Single();
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, username));
                claims.Add(new Claim(ClaimTypes.Role, "User"));
                if (username.Equals("admin"))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }
                return new ClaimsPrincipal(new ClaimsIdentity(claims));
            }
        }
    }
}