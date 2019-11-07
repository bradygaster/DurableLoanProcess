using System;
using System.Collections.Generic;
using DurableLoans.DomainModel;

namespace DurableLoans.LoanOfficerNotificationService.Services
{
    public class LoanApplicationProxy
    {
        public LoanApplicationProxy()
        {
            ReceivedLoans = new List<LoanApplicationResult>();
        }

        public List<LoanApplicationResult> ReceivedLoans { get; }

        public void SendLoanApplicationToOfficer(LoanApplicationResult loanApplication)
        {
            ReceivedLoans.Add(loanApplication);
        }
    }
}
