using Newtonsoft.Json;

namespace RattusAPI.Models
{
    public class StartGameRequest
    {
        [JsonProperty("players")]
        public string[] Players { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}