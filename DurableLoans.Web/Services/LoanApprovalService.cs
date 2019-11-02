using System.Collections.Generic;
using System.Threading.Tasks;
using DurableLoans.LoanOfficerNotificationService;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace DurableLoans.Web.Services
{
    public class LoanApprovalService
    {
        public LoanApprovalService(IConfiguration configuration,
            ILogger<LoanApprovalService> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<LoanApprovalService> Logger { get; }
        public LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierClient LoanApplicationReceivedNotifierClient { get; private set; }

        public IAsyncEnumerable<LoanApplicationReceived> GetIncomingLoanApplications()
        {
            Logger.LogInformation("Getting received loans");
            var grpcUrl = Configuration["LoanApprovalService:BaseAddress"];
            using var channel = GrpcChannel.ForAddress(grpcUrl);
            LoanApplicationReceivedNotifierClient = new LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierClient(channel);
            Logger.LogInformation("Returning received loans");

            return LoanApplicationReceivedNotifierClient
                        .GetLoanApplicationStream(new Empty())
                            .ResponseStream
                                .ReadAllAsync();
        }
    }
}