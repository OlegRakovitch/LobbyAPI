using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using LobbyAPI.Views;
using LobbyEngine;
using LobbyAPI.Models;
using Microsoft.AspNetCore.Http;
using LobbyAPI.Authentication;
using System.Threading.Tasks;
using LobbyAPI.Lobby;
using LobbyEngine.Controllers.Statuses;

namespace LobbyAPI.Controllers
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
                    IsOwner = user.Equals(room.Owner),
                    GameType = room.GameType
                };
                if (players.Contains(user))
                {
                    roomView.Players = players.Select(player => player.Username).ToArray();
                    roomView.Owner = room.Owner.Username;
                    roomView.GameId = room.GameId;
                }
                return roomView;
            });
        }

        [RequireRole(Roles.User)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody]CreateRequest data)
        {
            RoomCreateStatus? result;
            try
            {
                result = await lobbyEngine.RoomController.CreateRoom(data.Name, data.GameType);
            }
            catch (ArgumentException)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            switch(result)
            {
                case LobbyEngine.Controllers.Statuses.RoomCreateStatus.OK:
                    return StatusCode(StatusCodes.Status201Created);
                case LobbyEngine.Controllers.Statuses.RoomCreateStatus.DuplicateName:
                case LobbyEngine.Controllers.Statuses.RoomCreateStatus.AlreadyInRoom:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [RequireRole(Roles.User)]
        [HttpPost("join")]
        public async Task<IActionResult> JoinRoom([FromBody]JoinRequest data)
        {
            RoomJoinStatus? result;
            try
            {
                result = await lobbyEngine.RoomController.JoinRoom(data.Name);
            }
            catch (ArgumentException)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            switch(result)
            {
                case LobbyEngine.Controllers.Statuses.RoomJoinStatus.OK:
                    return StatusCode(StatusCodes.Status200OK);
                case LobbyEngine.Controllers.Statuses.RoomJoinStatus.AlreadyInRoom:
                case LobbyEngine.Controllers.Statuses.RoomJoinStatus.RoomIsFull:
                case LobbyEngine.Controllers.Statuses.RoomJoinStatus.RoomNotFound:
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
                case LobbyEngine.Controllers.Statuses.RoomLeaveStatus.OK:
                    return StatusCode(StatusCodes.Status200OK);
                case LobbyEngine.Controllers.Statuses.RoomLeaveStatus.GameInProgress:
                case LobbyEngine.Controllers.Statuses.RoomLeaveStatus.NotInRoom:
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
                case LobbyEngine.Controllers.Statuses.GameStartStatus.OK:
                    return StatusCode(StatusCodes.Status202Accepted);
                case LobbyEngine.Controllers.Statuses.GameStartStatus.GameInProgress:
                case LobbyEngine.Controllers.Statuses.GameStartStatus.NotAnOwner:
                case LobbyEngine.Controllers.Statuses.GameStartStatus.NotEnoughPlayers:
                case LobbyEngine.Controllers.Statuses.GameStartStatus.NotInRoom:
                    return StatusCode(StatusCodes.Status403Forbidden);
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
