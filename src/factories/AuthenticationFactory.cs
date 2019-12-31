using System;
using RattusAPI.Authentication;

namespace RattusAPI.Factories
{
    public static class AuthenticationFactory
    {
        public static Type Find(string authenticationTypeName)
        {
            switch(authenticationTypeName)
            {
                case "Trusted":
                    return typeof(TrustedAuthentication);
                case "JWT":
                    return typeof(JWTAuthentication);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}