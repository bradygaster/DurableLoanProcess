using System;

namespace DurableLoans.LoanProcess
{
    public class LoanApplication
    {
        public string CustomerName { get; set; }
        public int LoanAmount { get; set; }
    }

    public class CreditAgencyRequest
    {
        public string AgencyName { get; set; }
        public LoanApplication Application { get; set; }
        public string AgencyId { get; set; }
    }

    public class CreditAgencyResult
    {
        public string AgencyId { get; set; }
        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
    }

    public class LoanApplicationResult
    {
        public LoanApplication Application { get; set; }
        public bool IsSuccess { get; set; }
    }
}
