using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionsTests
{
    public static class LoanApprovalProcess
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "dashboard")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(HttpStart))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            var json = await req.Content.ReadAsStringAsync();
            var loanApplication = JsonConvert.DeserializeObject<LoanApplication>(json);

            string instanceId = await starter.StartNewAsync(nameof(Run), loanApplication);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        
        [FunctionName(nameof(Run))]
        public static async Task<LoanApplicationResult> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages, 
            ILogger logger)
        {
            var loanApplication = context.GetInput<LoanApplication>();
            var agencyTasks = new List<Task<CreditAgencyResult>>();
            var agencies = new List<CreditAgencyRequest>();
            var results = new CreditAgencyResult[]{};

            logger.LogWarning($"Status of application for {loanApplication.CustomerName} for {loanApplication.LoanAmount}: Checking with agencies.");
            
            // start the process and perform initial validation
            var loanStarted = await context.CallActivityAsync<bool>(nameof(StartLoanApprovalProcess), loanApplication);

            // fan out and check the credit agencies
            if(loanStarted)
            {
                agencies.AddRange(new CreditAgencyRequest[] {
                    new CreditAgencyRequest { AgencyName = "Contoso, Ltd.", AgencyId = "contoso", Application = loanApplication },
                    new CreditAgencyRequest { AgencyName = "Fabrikam, Inc.", AgencyId = "fabrikam", Application = loanApplication },
                    new CreditAgencyRequest { AgencyName = "Woodgrove Bank", AgencyId = "woodgrove", Application = loanApplication }
                });

                foreach (var agency in agencies)
                {
                    agencyTasks.Add(context.CallActivityAsync<CreditAgencyResult>(nameof(CheckCreditAgency), agency));
                }

                await dashboardMessages.AddAsync(new SignalRMessage
                {
                    Target = "agencyCheckPhaseStarted",
                    Arguments = new object[] { }
                });

                results = await Task.WhenAll(agencyTasks);

                await dashboardMessages.AddAsync(new SignalRMessage
                {
                    Target = "agencyCheckPhaseCompleted",
                    Arguments = new object[] { !(results.Any(x => x.IsApproved == false)) }
                });
            }

            var response = new LoanApplicationResult
            {
                Application = loanApplication,
                IsSuccess = loanStarted && !(results.Any(x => x.IsApproved == false))
            };

            logger.LogWarning($"Agency checks result with {response.IsSuccess} for loan amount of {response.Application.LoanAmount} to customer {response.Application.CustomerName}");

            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "loanApplicationComplete",
                Arguments = new object[] { response }
            });
            
            return response;
        }

        [FunctionName(nameof(StartLoanApprovalProcess))]
        public async static Task<bool> StartLoanApprovalProcess(
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages,
            [ActivityTrigger] LoanApplication loanApplication,
            ILogger log)
        {
            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "loanApplicationStart",
                Arguments = new object[] { loanApplication }
            });

            await Task.Delay(new Random().Next(3000, 6000)); // simulate variant processing times

            var result = loanApplication.LoanAmount < 10000;

            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "loanApplicationReceived",
                Arguments = new object[] { loanApplication, result }
            });

            return result;
        }
        
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
            await Task.Delay(rnd.Next(3000, 6000)); // simulate variant processing times

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

    public class LoanApplication
    {
        public string CustomerName { get; set; }
        public int LoanAmount { get; set; }
    }

    public class CreditAgencyRequest
    {
        public string AgencyName { get; set; }
        public LoanApplication Application { get; set; }
        public string AgencyId { get; set; }
    }

    public class CreditAgencyResult
    {
        public string AgencyId { get; set; }
        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
    }

    public class LoanApplicationResult
    {
        public LoanApplication Application { get; set; }
        public bool IsSuccess { get; set; }
    }
}