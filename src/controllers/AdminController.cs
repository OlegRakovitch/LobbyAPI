using Microsoft.AspNetCore.Mvc;
using RattusAPI.Authentication;
using RattusAPI.LobbyEngine;
using RattusEngine;
using RattusEngine.Models;

namespace RattusAPI.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        ILobbyEngine lobbyEngine;
        public AdminController(ILobbyEngineProvider engine)
        {
            lobbyEngine = engine;
        }

        [RequireRole(Roles.Admin)]
        [HttpPost("reset")]
        public async void ResetData()
        {
            await lobbyEngine.Context.Storage.DeleteAll<Room>();
        }
    }
}