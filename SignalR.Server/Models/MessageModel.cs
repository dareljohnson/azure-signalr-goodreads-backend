using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Server.Models
{
    public class MessageModel
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public string LastEvent { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
