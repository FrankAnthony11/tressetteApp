using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TresetaApp.Models;

namespace TresetaApp.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {

        [HttpGet("[action]")]
        public IActionResult GenerateGame()
        {
         //   var game=new Game("d","f");
            return Ok("");
        }

    }
}
