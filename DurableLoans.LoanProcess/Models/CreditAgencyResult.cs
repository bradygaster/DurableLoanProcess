using DurableLoans.DomainModel;

namespace DurableLoans.LoanProcess.Models
{
    public class CreditAgencyResult
    {
        public string AgencyId { get; set; }
        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
    }
}
