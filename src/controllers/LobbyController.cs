using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RattusAPI.Views;
using RattusEngine;

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

        [HttpGet]
        public IEnumerable<RoomView> GetRooms()
        {
            throw new NotImplementedException();
        }

        [HttpPost("create")]
        public void CreateRoom([FromBody]string value)
        {
            throw new NotImplementedException();
        }

        [HttpPost("join")]
        public void JoinRoom([FromBody]string value)
        {
            throw new NotImplementedException();
        }

        [HttpPost("leave")]
        public void LeaveRoom()
        {
            throw new NotImplementedException();
        }

        [HttpPost("start")]
        public void StartGame()
        {
            throw new NotImplementedException();
        }
    }
}
