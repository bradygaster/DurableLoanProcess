using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using DurableLoans.DomainModel;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(Receive))]
        public async static Task<bool> Receive(
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages,
            [ActivityTrigger] LoanApplication loanApplication)
        {
            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "loanApplicationStart",
                Arguments = new object[] { loanApplication }
            });

            await Task.Delay(new Random().Next(1000, 3000)); // simulate variant processing times

            bool result = loanApplication.LoanAmount.Amount < 10000;

            await dashboardMessages.AddAsync(new SignalRMessage
            {
                Target = "loanApplicationReceived",
                Arguments = new object[] { loanApplication, result }
            });

            return result;
        }
    }
}
