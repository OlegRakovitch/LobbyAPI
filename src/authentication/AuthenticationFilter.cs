using Microsoft.AspNetCore.Mvc.Filters;

namespace RattusAPI.Authentication
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        IAuthentication authentication;
        public AuthenticationFilter(IAuthentication authentication)
        {
            this.authentication = authentication;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.User = authentication.AuthenticateUser(context);
        }
    }
}