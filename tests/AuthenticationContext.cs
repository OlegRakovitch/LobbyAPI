namespace RattusAPI.Tests
{
    public class AuthenticationContext
    {
        public static AuthenticationContext Empty = new AuthenticationContext(string.Empty);
        
        public readonly string Username;
        private AuthenticationContext(string username)
        {
            Username = username;
        }

        public static AuthenticationContext Create(string username)
        {
            return new AuthenticationContext(username);
        }
    }
}