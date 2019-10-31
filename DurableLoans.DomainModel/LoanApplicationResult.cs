using System.Collections.Generic;

namespace DurableLoans.DomainModel
{
    public class LoanApplicationResult
    {
        public LoanApplicationResult()
        {
            AgencyResults = new List<AgencyCheckResult>();
        }

        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
        public List<AgencyCheckResult> AgencyResults { get; set; }
    }

    public class AgencyCheckResult
    {
        public bool IsApproved { get; set; }
        public string AgencyName { get; set; }
    }
}
