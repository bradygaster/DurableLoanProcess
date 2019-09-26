using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    [Route("api/[controller]")]
    public class ReceiveLoan : Controller
    {
        [HttpPost]
        public void Post([FromBody] LoanApplication loanApplication)
        {
        }
    }
}
