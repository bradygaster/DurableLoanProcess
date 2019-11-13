using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DurableLoans.LoanOffice.InboxProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger,
            DataLayer dataLayer,
            InboxQueue inboxQueue)
        {
            _logger = logger;
            DataLayer = dataLayer;
            InboxQueue = inboxQueue;
        }

        public DataLayer DataLayer { get; }
        public InboxQueue InboxQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var loanApplication = InboxQueue.DequeueLoanApplicationResult();
                if(loanApplication != null)
                {
                    _logger.LogInformation($"Saving loan app from " +
                        $"{loanApplication.Application.Applicant.LastName} " +
                        $"for {loanApplication.Application.LoanAmount.Amount}.");

                    await DataLayer.SaveLoan(loanApplication);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
