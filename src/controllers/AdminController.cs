using Microsoft.AspNetCore.Mvc;
using RattusAPI.Authentication;
using RattusEngine;
using RattusEngine.Models;

namespace RattusAPI.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        IApplication application;
        public AdminController(IApplication application)
        {
            this.application = application;
        }

        [RequireRole(Roles.Admin)]
        [HttpPost("reset")]
        public void ResetData()
        {
            application.Context.Storage.DeleteAll<Room>();
        }
    }
}