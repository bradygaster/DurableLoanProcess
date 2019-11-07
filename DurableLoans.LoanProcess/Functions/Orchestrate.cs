using DurableLoans.DomainModel;
using DurableLoans.LoanProcess.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(Orchestrate))]
        public static async Task<LoanApplicationResult> Orchestrate(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages, 
            ILogger logger)
        {
            var loanApplication = context.GetInput<LoanApplication>();
            var agencyTasks = new List<Task<CreditAgencyResult>>();
            var agencies = new List<CreditAgencyRequest>();
            var results = new CreditAgencyResult[]{};

            logger.LogWarning($"Status of application for {loanApplication.Applicant.ToString()} for {loanApplication.LoanAmount}: Checking with agencies.");

            // start the process and perform initial validation
            bool loanStarted = await context.CallActivityAsync<bool>(nameof(Receive), loanApplication);

            // fan out and check the credit agencies
            if(loanStarted)
            {
                agencies.AddRange(new CreditAgencyRequest[] {
                    new CreditAgencyRequest { AgencyName = "Contoso, Ltd.", AgencyId = "contoso", Application = loanApplication },
                    new CreditAgencyRequest { AgencyName = "Fabrikam, Inc.", AgencyId = "fabrikam", Application = loanApplication },
                    new CreditAgencyRequest { AgencyName = "Woodgrove Bank", AgencyId = "woodgrove", Application = loanApplication },
                });

                foreach (var agency in agencies)
                {
                    agencyTasks.Add(context.CallActivityAsync<CreditAgencyResult>(nameof(CheckCreditAgency), agency));
                }

                await context.CallActivityAsync(nameof(SendDashboardMessage), new SignalRMessage
                {
                    Target = "agencyCheckPhaseStarted",
                    Arguments = new object[] { }
                });

                // wait for all the agencies to return their results
                results = await Task.WhenAll(agencyTasks);

                await context.CallActivityAsync(nameof(SendDashboardMessage), new SignalRMessage
                {
                    Target = "agencyCheckPhaseCompleted",
                    Arguments = new object[] { !(results.Any(x => x.IsApproved == false)) }
                });
            }

            var loanApplicationResult = new LoanApplicationResult
            {
                IsApproved = loanStarted && !(results.Any(x => x.IsApproved == false)),
                Application = loanApplication
            };

            logger.LogWarning($"Agency checks result with {loanApplicationResult.IsApproved} for loan amount of {loanApplication.LoanAmount.Amount} to customer {loanApplication.Applicant.ToString()}");

            foreach (var agencyResult in results)
            {
                loanApplicationResult.AgencyResults.Add(new AgencyCheckResult
                {
                    AgencyName = agencyResult.AgencyId,
                    IsApproved = agencyResult.IsApproved
                });
            }

            // send the loan for final human validation
            logger.LogInformation($"Sending loan application for {loanApplicationResult.Application.Applicant.FirstName} {loanApplicationResult.Application.Applicant.LastName} for approval");

            var json = System.Text.Json.JsonSerializer.Serialize<LoanApplicationResult>(loanApplicationResult, 
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            logger.LogInformation(json);

            var url = Environment.GetEnvironmentVariable("LoanOfficerServiceUrl");
            logger.LogInformation(url);

            var request = new DurableHttpRequest(
                HttpMethod.Post,
                new Uri(url),
                headers: new Dictionary<string, StringValues>{{ "Content-Type", "application/json"}},
                content: json,
                asynchronousPatternEnabled: false
            );

            DurableHttpResponse restartResponse = await context.CallHttpAsync(request);

            logger.LogInformation($"Status code returned: {restartResponse.StatusCode}");

            if(restartResponse.StatusCode == HttpStatusCode.Accepted)
            {
                await context.CallActivityAsync(nameof(SendDashboardMessage), new SignalRMessage
                {
                    Target = "loanApplicationComplete",
                    Arguments = new object[] { loanApplicationResult }
                });
            }
            
            return loanApplicationResult;
        }
    }
}