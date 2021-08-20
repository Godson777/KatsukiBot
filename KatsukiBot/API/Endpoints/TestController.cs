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
        
        public class Message {
            public string msg { get; set; }
        }

        [HttpPost("msg")]
        [ApiKey]
        //use [FromHeader] on a param to get it from the header instead of the url
        public async Task<IActionResult> SendMessage([FromBody] Message message) {
            var ch = await Program.DiscordShards[0].Discord.GetChannelAsync(866182677690187786);
            await ch.SendMessageAsync(message.msg);
            Program.Twitch.client.SendMessage("GodsonTM", message.msg);
            return Ok("message sent");
        }
    }
}
