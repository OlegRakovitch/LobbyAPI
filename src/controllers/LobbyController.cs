using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RattusAPI.Views;
using RattusEngine;
using RattusAPI.Models;
using Microsoft.AspNetCore.Http;
using RattusAPI.Authentication;
using System.Threading.Tasks;
using RattusAPI.LobbyEngine;

namespace RattusAPI.Controllers
{
    [Route("api/[controller]")]
    public class LobbyController : Controller
    {
        readonly ILobbyEngine lobbyEngine;
        public LobbyController(ILobbyEngineProvider engine)
        {
            lobbyEngine = engine;
        }

        [RequireRole(Roles.User)]
        [HttpGet]
        public async Task<IEnumerable<RoomView>> GetRooms()
        {
            var user = lobbyEngine.Context.GetUser();
            return (await lobbyEngine.RoomController.GetRooms()).Select(room =>
            {
                var players = room.Players;
                var roomView = new RoomView 
                {
                    Name = room.Name,
                    State = room.Status.ToString(),
                    PlayersCount = players.Count(),
                    IsOwner = user.Equals(room.Owner)
                };
                if (players.Contains(user))
                {
                    roomView.Players = players.Select(player => player.Username).ToArray();
                    roomView.Owner = room.Owner.Username;
                }
                return roomView;
            });
        }

        [RequireRole(Roles.User)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody]CreateRequest data)
        {
            var result = await lobbyEngine.RoomController.CreateRoom(data.Name, data.GameType);
            switch(result)
            {
                case RattusEngine.Controllers.Statuses.RoomCreateStatus.OK:
                    return StatusCode(StatusCodes.Status201Created);
                case RattusEngine.Controllers.Statuses.RoomCreateStatus.DuplicateName:
                case RattusEngine.Controllers.Statuses.RoomCreateStatus.AlreadyInRoom:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [RequireRole(Roles.User)]
        [HttpPost("join")]
        public async Task<IActionResult> JoinRoom([FromBody]JoinRequest data)
        {
            var result = await lobbyEngine.RoomController.JoinRoom(data.Name);
            switch(result)
            {
                case RattusEngine.Controllers.Statuses.RoomJoinStatus.OK:
                    return StatusCode(StatusCodes.Status200OK);
                case RattusEngine.Controllers.Statuses.RoomJoinStatus.AlreadyInRoom:
                case RattusEngine.Controllers.Statuses.RoomJoinStatus.RoomIsFull:
                case RattusEngine.Controllers.Statuses.RoomJoinStatus.RoomNotFound:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [RequireRole(Roles.User)]
        [HttpPost("leave")]
        public async Task<IActionResult> LeaveRoom()
        {
            var result = await lobbyEngine.RoomController.LeaveRoom();
            switch(result)
            {
                case RattusEngine.Controllers.Statuses.RoomLeaveStatus.OK:
                    return StatusCode(StatusCodes.Status200OK);
                case RattusEngine.Controllers.Statuses.RoomLeaveStatus.GameInProgress:
                case RattusEngine.Controllers.Statuses.RoomLeaveStatus.NotInRoom:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [RequireRole(Roles.User)]
        [HttpPost("start")]
        public async Task<IActionResult> StartGame()
        {
            var result = await lobbyEngine.RoomController.StartGame();
            switch(result)
            {
                case RattusEngine.Controllers.Statuses.GameStartStatus.OK:
                    return StatusCode(StatusCodes.Status202Accepted);
                case RattusEngine.Controllers.Statuses.GameStartStatus.GameInProgress:
                case RattusEngine.Controllers.Statuses.GameStartStatus.NotAnOwner:
                case RattusEngine.Controllers.Statuses.GameStartStatus.NotEnoughPlayers:
                case RattusEngine.Controllers.Statuses.GameStartStatus.NotInRoom:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
