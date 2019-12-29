namespace RattusAPI.Tests
{
    public class AuthenticationContext
    {
        public static AuthenticationContext Empty = new AuthenticationContext(string.Empty, string.Empty);
        
        public readonly string HeaderName;
        public readonly string HeaderValue;

        public AuthenticationContext(string headerName, string headerValue)
        {
            HeaderName = headerName;
            HeaderValue = headerValue;
        }
    }
}