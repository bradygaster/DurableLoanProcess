using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using DurableLoans.DomainModel;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(SendForApproval))]
        public async static Task<bool> SendForApproval(
            [ActivityTrigger] LoanApplicationResult loanApplicationResult,
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages,
            ILogger logger)
        {
            logger.LogInformation($"Sending loan application for {loanApplicationResult.Application.Applicant.FirstName} {loanApplicationResult.Application.Applicant.LastName} for approval");

            var json = System.Text.Json.JsonSerializer.Serialize<LoanApplicationResult>(loanApplicationResult);
            logger.LogDebug(json);

            var url = Environment.GetEnvironmentVariable("LoanOfficerServiceUrl");
            logger.LogDebug(url);

            var request = new DurableHttpRequest(
                HttpMethod.Post,
                new Uri(url),
                null,
                json
            );
/*
            DurableHttpResponse restartResponse = await context.CallHttpAsync(request);
            
            if (restartResponse.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
*/
            return true;
        }
    }
}
