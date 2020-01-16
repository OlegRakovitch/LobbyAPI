using Microsoft.AspNetCore.Mvc;

namespace LobbyAPI.Authentication
{
    public class RequireRoleAttribute: TypeFilterAttribute
    {
        public RequireRoleAttribute(Roles role) : base(typeof(RequireRoleFilter))
        {
            Arguments = new object[] { role };
        }
    }
}