using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableLoans.LoanProcess
{
    public static partial class Functions
    {
        [FunctionName(nameof(SendDashboardMessage))]
        public async static Task SendDashboardMessage(
            [SignalR(HubName = "dashboard")] IAsyncCollector<SignalRMessage> dashboardMessages,
            [ActivityTrigger] SignalRMessage message)
        {
            await dashboardMessages.AddAsync(message);
        }
    }
}
