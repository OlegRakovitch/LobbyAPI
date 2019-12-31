using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RattusAPI.Authentication
{
    public class RequireRoleFilter: IAuthorizationFilter
    {
        private readonly Roles role;
 
        public RequireRoleFilter(Roles role)
        {            
            this.role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!CheckUserPermission(context.HttpContext.User, role))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    
        private bool CheckUserPermission(ClaimsPrincipal user, Roles role)
        {
            return user.Identity != null && user.Claims != null && user.HasClaim(ClaimTypes.Role, role.ToString());
        }
    }
}