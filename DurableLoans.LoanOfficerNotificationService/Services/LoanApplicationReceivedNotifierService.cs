using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableLoans.DomainModel;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    public class LoanApplicationReceivedNotifierService
        : LoanApplicationReceivedNotifier.LoanApplicationReceivedNotifierBase
    {
        public LoanApplicationReceivedNotifierService(ILogger<LoanApplicationReceivedNotifierService> logger)
        {
            Logger = logger;
        }

        public ILogger<LoanApplicationReceivedNotifierService> Logger { get; }

#pragma warning disable CS1998
        public override async Task GetLoanApplicationStream(Empty request, 
            IServerStreamWriter<LoanApplicationReceived> responseStream, 
            ServerCallContext context)
#pragma warning restore CS1998
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                // todo: get the non-finalized loans from cosmos and send back
                var tmp = new List<LoanApplicationResult>();

                tmp.ForEach(async loanApp => 
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

                    Logger.LogInformation($"Returning loan application for {receivedLoan.CustomerName}");
                    await responseStream.WriteAsync(receivedLoan);
                    await Task.Delay(TimeSpan.FromSeconds(2));
                });
            }
        }
    }
}