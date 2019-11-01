using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DurableLoans.DomainModel;
using System.Text.Json;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    [Route("[controller]")]
    public class LoanApplicationController : Controller
    {
        public LoanApplicationController(LoanApplicationProxy loanApplicationProxy,
            ILogger<LoanApplicationController> logger)
        {
            LoanApplicationProxy = loanApplicationProxy;
            Logger = logger;
        }

        public LoanApplicationProxy LoanApplicationProxy { get; }
        public ILogger<LoanApplicationController> Logger { get; }

        [HttpPost]
        public ActionResult Post([FromBody] LoanApplicationResult loanApplicationResult)
        {
            var json = JsonSerializer.Serialize<LoanApplicationResult>(loanApplicationResult, 
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            Logger.LogDebug(json);
            
            //LoanApplicationProxy.SendLoanApplicationToOfficer(loanApplication);
            return Accepted();
        }
    }
}
