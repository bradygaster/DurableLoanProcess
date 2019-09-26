using System;
using System.Collections.Generic;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    public class LoanApplicationProxy
    {
        public LoanApplicationProxy()
        {
            ReceivedLoans = new List<LoanApplication>();
        }

        public List<LoanApplication> ReceivedLoans { get; }

        public void SendLoanApplicationToOfficer(LoanApplication loanApplication)
        {
            ReceivedLoans.Add(loanApplication);
        }
    }
}
