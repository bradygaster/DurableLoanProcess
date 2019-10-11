using System.ComponentModel.DataAnnotations;

namespace DurableLoans.Web.ViewModels
{
    public class Applicant
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public override string ToString() =>
            $"{LastName}, {FirstName}";
    }
}
