using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(negotiate))]
        public static SignalRConnectionInfo negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "dashboard")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}