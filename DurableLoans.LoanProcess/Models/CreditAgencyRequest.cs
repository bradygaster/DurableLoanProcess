﻿using DurableLoans.DomainModel;

namespace DurableLoans.LoanProcess.Models
{
    public class CreditAgencyRequest
    {
        public string AgencyId { get; set; }
        public string AgencyName { get; set; }
        public LoanApplication Application { get; set; }
    }
}