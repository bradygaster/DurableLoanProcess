using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace DurableLoans.LoanOffice.ToBeApproved
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        webBuilder.ConfigureKestrel(options =>
                        {
                            options.ListenLocalhost(5003, o => o.Protocols = HttpProtocols.Http2);
                            options.ListenLocalhost(5005, o => o.Protocols = HttpProtocols.Http1);
                        });
                    }
                    
                    webBuilder.UseStartup<Startup>();
                });
    }
}
