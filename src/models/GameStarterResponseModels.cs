using Newtonsoft.Json;

namespace RattusAPI.Models
{
    public class StartGameResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}