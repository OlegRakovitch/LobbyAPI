using Newtonsoft.Json;

namespace RattusAPI.Models
{
    public class NameRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}