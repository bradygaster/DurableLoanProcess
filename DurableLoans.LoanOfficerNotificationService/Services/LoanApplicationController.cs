using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    [Route("api/[controller]")]
    public class LoanApplicationController : Controller
    {
        public LoanApplicationController(LoanApplicationProxy loanApplicationProxy)
        {
            LoanApplicationProxy = loanApplicationProxy;
        }

        public LoanApplicationProxy LoanApplicationProxy { get; }

        [HttpPost]
        public ActionResult Post([FromBody] LoanApplication loanApplication)
        {
            LoanApplicationProxy.SendLoanApplicationToOfficer(loanApplication);
            return Accepted();
        }
    }
}
