using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalR.Server
{
    internal static class Configuration
    {
        // Good Reads Base Url
        public static string baseUrl = "GoodReadsWebsiteBaseUrl";
        public static string baseUrlString => Environment.GetEnvironmentVariable(baseUrl);

        // SignalR
        public static string signalRConnectionString = "AzureSignalRConnectionString";
        public static string signalRConnection => Environment.GetEnvironmentVariable(signalRConnectionString);

        // SignalR Host Url
        public static string hostUrl = "AzureSignalRHostUrl";
        public static string signalRHostUrl => Environment.GetEnvironmentVariable(hostUrl);

        // Azure Cosmos DB
        public static string cosmosDb = "AzureCosmosDBConnectionString";
        public static string cosmosDbConnection => Environment.GetEnvironmentVariable(cosmosDb);

        // Azure Service Bus Queue
        public static string sbQueue = "AzureServiceBusQueueConnectionString";
        public static string sbQueueConnection => Environment.GetEnvironmentVariable(sbQueue);
    }
}
