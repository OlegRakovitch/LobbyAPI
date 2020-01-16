using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using LobbyAPI.Models;

namespace LobbyAPI.Tests.Controllers
{
    [Route("api/gamestarter")]
    public class GameStarterController : Controller
    {
        [HttpPost]
        public string StartGame([FromBody]StartGameRequest data)
        {
            return JsonConvert.SerializeObject(new {
                Id = $"{data.Type}-[${string.Join(',', data.Players)}]"
            });
        }
    }
}