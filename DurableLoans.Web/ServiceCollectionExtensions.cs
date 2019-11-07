using System;
using DurableLoans.LoanOfficerNotificationService;
using DurableLoans.Web.Services;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;

namespace DurableLoans.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoanOfficerClient(this IServiceCollection services,
            Action<GrpcClientFactoryOptions> configure)
        {
            services.AddGrpcClient<LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierClient>(configure);
            services.AddScoped<LoanApprovalService>();
            return services;
        }
    }
}