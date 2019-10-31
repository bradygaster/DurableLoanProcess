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
                        CustomerName = string.Format($"{loanApp.Application.Applicant.FirstName} {loanApp.Application.Applicant.LastName}"),
                        LoanAmount = loanApp.Application.LoanAmount.Amount
                    };

                    receivedLoan.AgencyResults.AddRange(
                        loanApp.AgencyResults.Select(x => 
                            new AgencyResultReceived {
                                AgencyId = x.AgencyName,
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