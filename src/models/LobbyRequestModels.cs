using Newtonsoft.Json;

namespace RattusAPI.Models
{
    public class JoinRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class CreateRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string GameType { get; set; }
    }
}