using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DurableLoans.DomainModel;
using System.Text.Json;

namespace DurableLoans.LoanOffice.Inbox
{
    [Route("[controller]")]
    public class LoanApplicationController : Controller
    {
        public LoanApplicationController(ILogger<LoanApplicationController> logger,
            InboxQueue inboxQueue)
        {
            Logger = logger;
            InboxQueue = inboxQueue;
        }

        public ILogger<LoanApplicationController> Logger { get; }
        public InboxQueue InboxQueue { get; }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LoanApplicationResult loanApplicationResult)
        {
            await InboxQueue.EnqueueLoanApplicationResult(loanApplicationResult);
            return Accepted();
        }
    }
}
