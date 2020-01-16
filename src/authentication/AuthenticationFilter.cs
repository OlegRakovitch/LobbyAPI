using Microsoft.AspNetCore.Mvc.Filters;

namespace LobbyAPI.Authentication
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        IAuthenticationProvider authentication;
        public AuthenticationFilter(IAuthenticationProvider authentication)
        {
            this.authentication = authentication;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.User = authentication.AuthenticateUser(context);
        }
    }
}