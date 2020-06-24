using System.ComponentModel.DataAnnotations;

namespace EMBC.ESS.Areas.Evacuees.Models
{
    public class Profile
    {
        [Display(Name = "Full Name"), Required]
        public string Name { get; set; }

        [Display(Name = "Date of Birth"), Required]
        public string DateOfBirth { get; set; }

        [Display(Name = "Home Address"), Required]
        public string Address { get; set; }
    }
}
