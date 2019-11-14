using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableLoans.DomainModel;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanOffice.ToBeApproved
{
    public class FeedService : Feed.FeedBase
    {
        public FeedService(ILogger<FeedService> logger,
            DataLayer dataLayer)
        {
            Logger = logger;
            DataLayer = dataLayer;
        }

        public ILogger<FeedService> Logger { get; }
        public DataLayer DataLayer { get; }

#pragma warning disable CS1998
        public override async Task Receive(Empty request, 
            IServerStreamWriter<FeedItem> responseStream, 
            ServerCallContext context)
#pragma warning restore CS1998
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var inbox = DataLayer
                        .GetInbox()
                        .Select(x => x.LoanApplication).ToList();

                inbox.ForEach(async loanApp => 
                {
                    var receivedLoan = new FeedItem
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
                });

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}