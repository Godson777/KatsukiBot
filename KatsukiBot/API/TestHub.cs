using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Helix;

namespace KatsukiBot.API {
    class TestHub : Hub {
        private readonly Random random = new Random();


        public Task RequestString() {
            return Clients.Group("balls").SendAsync("UpdateLabel", "no");
        }

        public override async Task OnConnectedAsync() {
            if (!Context.GetHttpContext().Request.Headers.TryGetValue("ApiKey", out var ApiKey)) {
                await Clients.Caller.SendAsync("RejectConnection", "No API Key was provided.");
                return;
            }

            var correctKey = "balls";

            if (!ApiKey.Equals(correctKey)) {
                await Clients.Caller.SendAsync("RejectConnection", "Provided key is not valid.");
                return;
            }
            
            if (ApiKey == correctKey) await Groups.AddToGroupAsync(Context.ConnectionId, "balls");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception) {
            
            var key = Context.GetHttpContext().Request.Headers["key"];
            if (key == "balls") await Groups.RemoveFromGroupAsync(Context.ConnectionId, "balls");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
