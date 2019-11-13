using System.Collections.Generic;
using System.Threading.Tasks;
using DurableLoans.LoanOfficerNotificationService;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Threading;

namespace DurableLoans.Web.Services
{
    public class LoanApprovalService
    {
        public LoanApprovalService(ILogger<LoanApprovalService> logger,
            LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierClient client)
        {
            Logger = logger;
            LoanOfficerClient = client;
        }

        public ILogger<LoanApprovalService> Logger { get; }
        public LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierClient LoanOfficerClient { get; private set; }

        public IAsyncEnumerable<LoanApplicationReceived> GetIncomingLoanApplications(CancellationToken token)
        {
            return LoanOfficerClient.GetLoanApplicationStream(
                new Empty(), 
                cancellationToken: token).ResponseStream.ReadAllAsync();
        }
    }
}