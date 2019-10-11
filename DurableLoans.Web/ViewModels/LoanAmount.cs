using DurableLoans.DomainModel;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DurableLoans.Web.ViewModels
{
    public class LoanAmount
    {
        [DisplayName("Loan Amount")]
        [Range(1, 100000)]
        public decimal Amount { get; set; } = 0.00m;

        public string CurrencyType { get; set; } = Constants.UsDollarSymbol;
    }
}
