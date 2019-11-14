using System;
using DurableLoans.Web.Services;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;
using DurableLoans.LoanOffice.ToBeApproved;

namespace DurableLoans.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoanOfficerClient(this IServiceCollection services,
            Action<GrpcClientFactoryOptions> configure)
        {
            services.AddGrpcClient<Feed.FeedClient>(configure);
            services.AddScoped<FeedClientService>();
            return services;
        }
    }
}