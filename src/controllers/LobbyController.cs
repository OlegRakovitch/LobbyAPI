using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RattusAPI.Views;
using RattusEngine;
using RattusAPI.Models;
using Microsoft.AspNetCore.Http;
using RattusAPI.Authentication;

namespace RattusAPI.Controllers
{
    [Route("api/[controller]")]
    public class LobbyController : Controller
    {
        readonly IApplication application;
        public LobbyController(IApplication application)
        {
            this.application = application;
        }

        [RequireRole(Roles.User)]
        [HttpGet]
        public IEnumerable<RoomView> GetRooms()
        {
            var user = application.Context.GetUser();
            return application.RoomController.GetRooms().Select(room =>
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
        public IActionResult CreateRoom([FromBody]NameRequest data)
        {
            var result = application.RoomController.CreateRoom(data.Name);
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
        public IActionResult JoinRoom([FromBody]NameRequest data)
        {
            var result = application.RoomController.JoinRoom(data.Name);
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
        public IActionResult LeaveRoom()
        {
            var result = application.RoomController.LeaveRoom();
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
        public IActionResult StartGame()
        {
            var result = application.RoomController.StartGame();
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
