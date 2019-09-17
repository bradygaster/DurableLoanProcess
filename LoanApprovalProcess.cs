using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionsTests
{
    public static class LoanApprovalProcess
    {

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
            ILogger logger)
        {
            var loanApplication = context.GetInput<LoanApplication>();
            var agencies = new List<CreditAgencyRequest>();
            var results = new List<CreditAgencyResult>();

            logger.LogWarning($"Status of application for {loanApplication.CustomerName} for {loanApplication.LoanAmount}: Checking with agencies.");
            
            agencies.AddRange(new CreditAgencyRequest[] {
                new CreditAgencyRequest { AgencyName = "Experian", Application = loanApplication },
                new CreditAgencyRequest { AgencyName = "TransUnion", Application = loanApplication },
                new CreditAgencyRequest { AgencyName = "Experian", Application = loanApplication }
            });

            foreach (var agency in agencies)
            {
                results.Add(
                    (await context.CallActivityAsync<CreditAgencyResult>(nameof(CheckCreditAgency), agency))
                );
            }

            var response = new LoanApplicationResult
            {
                Application = loanApplication,
                IsSuccess = !(results.Any(x => x.IsApproved == false))
            };

            logger.LogWarning($"Agency checks result with {response.IsSuccess} for loan amount of {response.Application.LoanAmount} to customer {response.Application.CustomerName}");
            
            return response;
        }
        
        [FunctionName(nameof(CheckCreditAgency))]
        public async static Task<CreditAgencyResult> CheckCreditAgency(
            [ActivityTrigger] CreditAgencyRequest request, 
            ILogger log)
        {
            log.LogWarning($"Checking agency {request.AgencyName} for customer {request.Application.CustomerName} for {request.Application.LoanAmount}");

            var rnd = new Random();
            await Task.Delay(rnd.Next(3000, 6000)); // simulate variant processing times

            var result = new CreditAgencyResult
            {
                IsApproved = true,
                Application = request.Application
            };

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
    }

    public class CreditAgencyResult
    {
        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
    }

    public class LoanApplicationResult
    {
        public LoanApplication Application { get; set; }
        public bool IsSuccess { get; set; }
    }
}