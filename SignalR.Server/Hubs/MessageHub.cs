using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalR.Server.Hubs
{
    public class MessageHub : DynamicHub
    {
        public override async Task OnConnectedAsync()
        {
            // Code here...


            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            // Code here...


            await base.OnDisconnectedAsync(ex);
        }

        private static string GetGroupName()
        {
            // Code here...


            var group = "";

            return group;

        }
    }
}
