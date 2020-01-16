using Newtonsoft.Json;

namespace LobbyAPI.Models
{
    public class StartGameResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}