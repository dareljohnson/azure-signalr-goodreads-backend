using Microsoft.Extensions.Logging;
using SignalR.Server.Models;
using System.Collections.Generic;

namespace SignalR.Server.Interfaces
{
    public interface IGoodReadsService
    {
        List<Book> GetBooksFromGoodReads(ILogger log, string userId, string bookShelf);
    }
}
