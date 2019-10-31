using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DurableLoans.DomainModel;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    [Route("api/[controller]")]
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
        public ActionResult Post([FromBody] LoanApplication loanApplication)
        {
            Logger.LogInformation($"Loan App received for {loanApplication.Applicant.FirstName} {loanApplication.Applicant.LastName}");
            
            //LoanApplicationProxy.SendLoanApplicationToOfficer(loanApplication);
            return Accepted();
        }
    }
}
