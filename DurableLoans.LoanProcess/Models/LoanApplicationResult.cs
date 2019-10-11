using DurableLoans.DomainModel;

namespace DurableLoans.LoanProcess.Models
{
    public class LoanApplicationResult
    {
        public LoanApplication Application { get; set; }
        public bool IsApproved { get; set; }
    }
}
