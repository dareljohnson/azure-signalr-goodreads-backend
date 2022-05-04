using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SignalR.Server.Hubs;
using SignalR.Server.Interfaces;
using SignalR.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Server.Services
{
    public class InvokeMessageService : IInvokeMessageService
    {
        private readonly IHubContext<MessageHub> _hubContext;

        public InvokeMessageService(IHubContext<MessageHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendGoodReadsResponseAsync(string target, List<Book> bookResponse)
        {
            var books = new List<Book>();
            

            foreach(var book in bookResponse)
            {
                books.Add(new Book
                {
                    Title = book.Title,
                    Author = book.Author,
                    ImagePath = book.ImagePath
                });
            }

            if (books != null)
                await _hubContext.Clients.All.SendCoreAsync(target, new[] { books });
        }
    }
}
