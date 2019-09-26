using System;
using System.Collections.Generic;

namespace DurableLoans.LoanOfficerNotificationService
{
    public class LoanApplication
    {
        public LoanApplication()
        {
            this.CreditAgencyResults = new List<CreditAgencyResult>();
        }

        public string CustomerName { get; set; }
        public int LoanAmount { get; set; }
        public List<CreditAgencyResult> CreditAgencyResults { get; set; }
    }

    public class CreditAgencyResult
    {
        public string AgencyId { get; set; }
        public bool IsApproved { get; set; }
    }
}
