using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class ViewModel : PageModel
    {
        public class ProfilViewModel
        {
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address")]
            public string Address { get; set; }
        }

        private readonly IRepository<Profile> repository;

        public ViewModel(IRepository<Profile> repository)
        {
            this.repository = repository;
        }

        [ViewData]
        public ProfilViewModel Profile { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var profile = await repository.GetByIdAsync(id);
            Profile = new ProfilViewModel
            {
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                Name = profile.Name
            };
            return Page();
        }
    }
}
