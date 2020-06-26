using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EMBC.ESS.Areas.Supporters.Models
{
    public class Profile
    {
        [HiddenInput]
        public string Id { get; set; }

        [Display(Name = "Full Name"), Required]
        public string Name { get; set; }

        [Display(Name = "Date of Birth"), Required]
        public string DateOfBirth { get; set; }

        [Display(Name = "Home Address"), Required]
        public string Address { get; set; }

        [Display(Name = "BC Services Card"), Required]
        public string Identity { get; set; }
    }
}
