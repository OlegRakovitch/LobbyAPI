using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RattusAPI.Authentication
{
    public interface IAuthentication
    {
        ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context);
    }
}