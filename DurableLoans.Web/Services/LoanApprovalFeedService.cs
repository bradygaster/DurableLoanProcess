using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Threading;
using DurableLoans.LoanOffice.ToBeApproved;

namespace DurableLoans.Web.Services
{
    public class FeedClientService
    {
        public FeedClientService(ILogger<FeedClientService> logger, 
            Feed.FeedClient client)
        {
            Logger = logger;
            ToBeApprovedFeedClient = client;
        }

        public ILogger<FeedClientService> Logger { get; }
        public Feed.FeedClient ToBeApprovedFeedClient { get; private set; }

        public IAsyncEnumerable<FeedItem> Receive(CancellationToken token)
        {
            return ToBeApprovedFeedClient.Receive(
                new Empty(), 
                cancellationToken: token
            ).ResponseStream.ReadAllAsync();
        }
    }
}