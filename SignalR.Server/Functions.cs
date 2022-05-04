using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.Mvc;
using SignalR.Server.Interfaces;
using System.Linq;
using Microsoft.Azure.Cosmos;
using SignalR.Server.Models;
using SignalR.Server.Common.Utils;
using System.Collections.Generic;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Microsoft.Azure.ServiceBus;

namespace SignalR.Server
{
    public class Functions
    {
        // Cosmos DB
        /*
        private static readonly string _connnectionString = Configuration.cosmosDbConnection;
        private static readonly CosmosClient _client = new CosmosClient(_connnectionString);
        const string _grDocuments = "GoodReads";
        const string _books = "Books";
        static readonly Database grDatabase = _client.GetDatabase(_grDocuments);
        static readonly Container grContainer = grDatabase.GetContainer(_books);
        */

        // Service Bus
        private static readonly string _sbCconnnectionString = Configuration.sbQueueConnection;
        static string queueName = "book-queue";
        

        private readonly IGoodReadsService _service;

        public Functions(IGoodReadsService goodReadsService)
        {
            _service = goodReadsService ?? throw new ArgumentNullException(nameof(goodReadsService));
        }

        [FunctionName("Negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalRConnectionInfo(HubName = "favoriteBooksHub")] SignalRConnectionInfo hubConnectionInfo,
            ILogger log)
        {
            try
            {
                log.LogInformation($"Returning Hub connection established at {DateTime.UtcNow} Utc" +
                hubConnectionInfo.Url + " " + hubConnectionInfo.AccessToken);

                return hubConnectionInfo;
            }
            catch (Exception ex)
            {
                log.LogError("Something went wrong during processing: ", ex);

                return null;
            }
        }

        [FunctionName("GetFavoriteBooks")]
        public async Task Run(
            [TimerTrigger("0/2 * * * * *")] TimerInfo timer, //https://crontab.guru/#0_*_*_*_*
            [CosmosDB("GoodReads", "Books", ConnectionStringSetting = "AzureCosmosDBConnectionString")] IEnumerable<Book> bookItem,
            [SignalR(HubName = "favoriteBooksHub")] IAsyncCollector<SignalRMessage> signalRMessage,
            [CosmosDB("GoodReads", "Books", ConnectionStringSetting = "AzureCosmosDBConnectionString")] IAsyncCollector<Book> documentsOut,
            ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now} or {DateTime.UtcNow} Utc");

                // Scrape recommended books from GoodReads website
                var fovoriteBooks = _service.GetBooksFromGoodReads(log, "115889861", "recommended");

                // Read historical book data 
                List<Book> books = new();
                var previousBooks = books.ToDictionary(b => b.ComputedHash);
                if (bookItem.Any())
                {
                    foreach (var book in bookItem)
                    {
                        books.Add(book);
                        Console.WriteLine(book.Title + " " + book.Author);
                    }
                    foreach (var book in books)
                    {
                        previousBooks.Add(book.ComputedHash, book);
                    }
                }

                //Store recommended books in Cosmos DB collection: Books
                if (fovoriteBooks.Any())
                {
                    foreach (var book in fovoriteBooks)
                    {
                        var bookHash = HashString.GetHashString((book.Title + book.Author + book.ImagePath), "1234");
                        if (books.Any() && previousBooks.ContainsKey(bookHash)) continue;

                        await documentsOut.AddAsync(new Book
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = book.Title,
                            Author = book.Author,
                            ImagePath = book.ImagePath,
                            ComputedHash = HashString.GetHashString((book.Title + book.Author + book.ImagePath), "1234"),
                            LastEvent = DomainEvent.Recommended.ToString(),
                            Created = DateTime.UtcNow.Date.ToString("O"),
                        });
                        Console.WriteLine("Inserted a record into Cosmos DB collection");

                        // Send message to Service Bus
                        ServiceBusClient client = new ServiceBusClient(_sbCconnnectionString);
                        ServiceBusSender sender = client.CreateSender(queueName);
                        MessageModel messageModel = new MessageModel
                        {
                            Id = Guid.NewGuid().ToString(),
                            Message = book.Author,
                            LastEvent = DomainEvent.Recommended.ToString(),
                            CreatedDate = DateTime.UtcNow.Date
                        };
                        ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(messageModel));
                        long sequenceNumber = await sender.ScheduleMessageAsync(message, DateTimeOffset.UtcNow.AddSeconds(1));
                        Console.WriteLine($"Scheduled a message with a sequence number: {sequenceNumber}");

                    }
                }

                await signalRMessage.AddAsync(new SignalRMessage
                {
                    Target = "myBooks",
                    Arguments = new[] { fovoriteBooks }
                });

            }
            catch (Exception ex)
            {
                log.LogError("Something went wrong during processing: ", ex);
            }
        }

        [FunctionName("TestBookList")]
        public async Task<IActionResult> TestBookList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [SignalR(HubName = "favoriteBooksHub")] IAsyncCollector<SignalRMessage> signalRMessage, 
            ILogger log)
        {
            try
            {
                log.LogInformation($"C# HTTP trigger function processed a request at: {DateTime.Now}");

                // Scrape recommended books from GoodReads website
                var fovoriteBooks = _service.GetBooksFromGoodReads(log, "115889861", "recommended");
  
                await signalRMessage.AddAsync(new SignalRMessage
                {
                    Target = "myBooks",
                    Arguments = new[] { fovoriteBooks }
                });

                return new OkObjectResult("Ok, great!");

            }
            catch (Exception ex)
            {
                log.LogError("Something went wrong during processing: ", ex);

                return new BadRequestObjectResult(ex.Message);
            }

        }

        [FunctionName("ServiceBusQueueTrigger")]
        public async Task GetQueueMessage(
        [ServiceBusTrigger("book-queue", Connection = "AzureServiceBusQueueConnectionString")]
        string message, 
        [SignalR(HubName = "favoriteBooksHub")] IAsyncCollector<SignalRMessage> signalRMessage, 
        ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message:> {message}");
            
            MessageModel output = JsonSerializer.Deserialize<MessageModel>(message);

            MessageModel messageOut = new MessageModel();
            messageOut.Id = output.Id;
            messageOut.Message = output.Message;
            messageOut.LastEvent = output.LastEvent;
            messageOut.CreatedDate = output.CreatedDate;


            Console.WriteLine($"Id: {messageOut.Id}");
            Console.WriteLine($"Message: {messageOut.Message}");
            Console.WriteLine($"LastEvent: {messageOut.LastEvent}");
            Console.WriteLine($"Date: {messageOut.CreatedDate}");

            await signalRMessage.AddAsync(new SignalRMessage
            {
                Target = "myToaster",
                Arguments = new[] { messageOut.LastEvent }
            });

            _ = Task.CompletedTask;
        }
    }
}
