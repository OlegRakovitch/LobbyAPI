using Microsoft.AspNetCore.Mvc;
using LobbyAPI.Authentication;
using LobbyAPI.Lobby;
using LobbyEngine;
using LobbyEngine.Models;

namespace LobbyAPI.Controllers
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