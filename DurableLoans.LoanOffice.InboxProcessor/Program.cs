using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DurableLoans.LoanOffice;
using Microsoft.Extensions.Configuration;

namespace DurableLoans.LoanOffice.InboxProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddAzureAppConfiguration(options =>
                    {
                        var azureAppConfigConnectionString =
                            hostingContext.Configuration["AzureAppConfigConnectionString"];
                        options.Connect(azureAppConfigConnectionString);
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddInboxQueueSupport();
                    services.AddInboxStorageSupport();
                    services.AddHostedService<Worker>();
                });
    }
}
