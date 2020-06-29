using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProfileReadModelRepository repository;

        public class ProfileViewModel
        {
            public string Id { get; set; }

            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address")]
            public string Address { get; set; }
        }

        public IndexModel(IProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        [ViewData]
        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public async Task<IActionResult> OnGetAsync(string firstName, string lastName, string dateOfBirth)
        {
            var profiles = (await repository.GetAllAsync())
                .Where(p => (string.IsNullOrWhiteSpace(firstName) || p.Name.StartsWith(firstName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(lastName) || p.Name.EndsWith(lastName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(dateOfBirth) || p.DateOfBirth.Equals(dateOfBirth, StringComparison.OrdinalIgnoreCase)));
            Profiles = profiles.Select(p => new ProfileViewModel
            {
                Id = p.Id.ToString(),
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                Name = p.Name
            });
            return Page();
        }
    }
}
