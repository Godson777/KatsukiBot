using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.API {
    [Route("test")]
    [ApiController]
    [Produces("application/json")]
    public class TestController : ControllerBase {
        [HttpGet("piss")]
        public IActionResult Get() {
            return Ok("lol");
        }

        [HttpPost("msg")]
        [ApiKey]
        public async Task<IActionResult> SendMessage([FromHeader] string msg) {
            var ch = await Program.Shards[0].Discord.GetChannelAsync(866182677690187786);
            await ch.SendMessageAsync(msg);
            return Ok("message sent");
        }
    }
}
