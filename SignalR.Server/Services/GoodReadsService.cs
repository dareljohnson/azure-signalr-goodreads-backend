using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SignalR.Server.Interfaces;
using SignalR.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SignalR.Server.Services
{
    public class GoodReadsService: IGoodReadsService
    {
        public List<Book> GetBooksFromGoodReads(ILogger log, string userId, string bookShelf)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrEmpty(bookShelf)) log.LogError("Invalid Parameters");

            string Url = $"{Configuration.baseUrlString}/{userId}?ref=nav_mybooks&shelf={bookShelf}";

            HtmlDocument htmlDoc = new();
            htmlDoc.OptionFixNestedTags = true;
            List<Book> books = new();

            using (HttpClient client = new())
            {
                using (HttpResponseMessage response = client.GetAsync(Url).Result)
                {
                    using (HttpContent content = response.Content)
                    {
                        string result = content.ReadAsStringAsync().Result;
                        htmlDoc.LoadHtml(result);

                        if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
                        {
                            // Handle any parse erros as required
                            log.LogError("Error on the Page parse!");
                        }
                        else
                        {
                            try {
                                if (htmlDoc.DocumentNode.SelectNodes("//tr[@class='bookalike review']") != null)
                                {
                                    for (int i = 0; i < htmlDoc.DocumentNode.SelectNodes("//tr[@class='bookalike review']").Count; i++)
                                    {
                                        books.Add(new Book
                                        {
                                            ImagePath = htmlDoc.DocumentNode.SelectNodes("//td[@class='field cover']")[i].ChildNodes[1].ChildNodes[1]
                                            .ChildNodes[1].ChildNodes[0].Attributes["src"].Value,
                                            Title = htmlDoc.DocumentNode.SelectNodes("//td[@class='field title']")[i].ChildNodes[1]
                                            .ChildNodes[1].Attributes["title"].Value,
                                            Author = htmlDoc.DocumentNode.SelectNodes("//td[@class='field author']")[i].ChildNodes[1]
                                            .ChildNodes[1].InnerText
                                        });
                                    }
                                }
                            }
                            catch(Exception ex) { 
                                log.LogError("Error parsing html element" + ex.Message);
                            }
                            
                        }
                    }
                }
            }

            return books;
        }
    }
}
