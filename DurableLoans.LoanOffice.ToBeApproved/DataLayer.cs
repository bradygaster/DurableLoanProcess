using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DurableLoans.DomainModel;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanOffice.ToBeApproved
{
    public class DataLayer
    {
        const string COSMOS_CONNECTION = "Azure:Cosmos:LoanInbox:ConnectionString";
        const string COSMOS_DATABASE = "Azure:Cosmos:LoanInbox:Database";
        const string COSMOS_CONTAINER = "Azure:Cosmos:LoanInbox:Collection";

        public DataLayer(IConfiguration configuration,
            ILogger<DataLayer> logger)
        {
            Configuration = configuration;
            Logger = logger;
            CosmosClient = new CosmosClient(
                Configuration[COSMOS_CONNECTION]
            );

            Task.WaitAll(Initialize());

            InboxContainer = CosmosClient
                .GetDatabase(Configuration[COSMOS_DATABASE])
                .GetContainer(Configuration[COSMOS_CONTAINER]);
        }

        public IConfiguration Configuration { get; }
        public ILogger<DataLayer> Logger { get; }
        public CosmosClient CosmosClient { get; }
        public Container InboxContainer { get; }

        private async Task Initialize()
        {
            var response = await CosmosClient.CreateDatabaseIfNotExistsAsync(
                Configuration[COSMOS_DATABASE]
                );

            if ((response.StatusCode == HttpStatusCode.Created)
                || (response.StatusCode == HttpStatusCode.OK))
            {
                await response.Database.CreateContainerIfNotExistsAsync(
                    Configuration[COSMOS_CONTAINER],
                    "/PartitionKey"
                );
            }
        }

        public IEnumerable<LoanApplicationResultRecord> GetInbox()
        {
            try
            {
                var items = InboxContainer.GetItemLinqQueryable<LoanApplicationResultRecord>(true).ToList();
                return items;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error getting inbox");
                return null;
            }
        }

        public async Task<bool> DeleteRecord(string id)
        {
            try
            {
                await InboxContainer.DeleteItemAsync<LoanApplicationResultRecord>(id,
                    new PartitionKey(LoanApplicationResultRecord.DEFAULT_PARTITION_KEY));

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error deleting record ");
                return false;
            }
        }
    }

    public static class DataLayerExtensions
    {
        public static IServiceCollection AddInboxStorageSupport(this IServiceCollection services)
        {
            services.AddSingleton<DataLayer>();
            return services;
        }
    }
}
