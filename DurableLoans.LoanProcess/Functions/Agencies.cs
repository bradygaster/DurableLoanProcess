using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(CheckCreditAgency))]
        public async static Task<CreditAgencyResult> CheckCreditAgency(
            [ActivityTrigger] CreditAgencyRequest request,
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages,
            ILogger log)
        {
            log.LogWarning($"Checking agency {request.AgencyName} for customer {request.Application.CustomerName} for {request.Application.LoanAmount}");

            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "agencyCheckStarted",
                Arguments = new object[] { request }
            });

            var rnd = new Random();
            await Task.Delay(rnd.Next(2000, 4000)); // simulate variant processing times

            var result = new CreditAgencyResult
            {
                IsApproved = !(request.AgencyName.Contains("Woodgrove") && request.Application.LoanAmount > 4999),
                Application = request.Application,
                AgencyId = request.AgencyId
            };

            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "agencyCheckComplete",
                Arguments = new object[] { result }
            });

            log.LogWarning($"Agency {request.AgencyName} {(result.IsApproved ? "APPROVED" : "DECLINED")} request by customer {request.Application.CustomerName} for {request.Application.LoanAmount}");

            return result;
        }
    }
}