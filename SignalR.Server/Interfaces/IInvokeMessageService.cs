using SignalR.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalR.Server.Interfaces
{
    public interface IInvokeMessageService
    {
       Task SendGoodReadsResponseAsync(string target, List<Book> bookResponse);
    }
}
