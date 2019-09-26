using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    public class LoanApplicationReceivedNotifierService
        : LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierBase
    {
        public LoanApplicationReceivedNotifierService(LoanApplicationProxy loanApplicationProxy)
        {
            LoanApplicationProxy = loanApplicationProxy;
        }

        public LoanApplicationProxy LoanApplicationProxy { get; }

        public override async Task GetLoanApplicationStream(Empty request, 
            IServerStreamWriter<LoanApplicationReceived> responseStream, 
            ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                LoanApplicationProxy.ReceivedLoans.ForEach(async loanApp => 
                {
                    var receivedLoan = new LoanApplicationReceived
                    {
                        CustomerName = loanApp.CustomerName,
                        LoanAmount = loanApp.LoanAmount
                    };

                    receivedLoan.AgencyResults.AddRange(
                        loanApp.CreditAgencyResults.Select(x => 
                            new AgencyResultReceived {
                                AgencyId = x.AgencyId,
                                IsApproved = x.IsApproved
                            })
                    );

                    await responseStream.WriteAsync(receivedLoan);
                });

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}