using Microsoft.AspNetCore.Mvc;

namespace RattusAPI.Authentication
{
    public class RequireRoleAttribute: TypeFilterAttribute
    {
        public RequireRoleAttribute(Roles role) : base(typeof(RequireRoleFilter))
        {
            Arguments = new object[] { role };
        }
    }
}