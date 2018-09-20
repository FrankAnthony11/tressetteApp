using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TresetaApp.Models;
using TresetaApp.Models.Dtos;

namespace TresetaApp.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly IMapper _mapper;

        public SampleDataController(IMapper mapper)
        {
            this._mapper = mapper;
        }
        [HttpGet("[action]")]
        public IActionResult GenerateGame()
        {
            var game = new Game("lol", new List<Player>(){
                new Player(new User("antonio","antonio")),
                new Player(new User("ivan","ivan"))
            }, 2);
            var gameDto=_mapper.Map<GameDto>(game);
            gameDto.MyCards=game.Players.First().Cards;
            return Ok(gameDto);
        }

    }
}
