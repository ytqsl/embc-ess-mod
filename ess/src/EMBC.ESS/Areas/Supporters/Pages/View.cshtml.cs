using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class ViewModel : PageModel
    {
        private readonly IRepository<Registration> repository;

        public class ProfileViewModel
        {
            public string Id { get; set; }

            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "home Address")]
            public string Address { get; set; }
        }

        public ViewModel(IRepository<Registration> repository)
        {
            this.repository = repository;
        }

        [ViewData]
        public ProfileViewModel Data { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var profile = await repository.GetByIdAsync(Guid.Parse(id));
            Data = new ProfileViewModel
            {
                Id = profile.Id.ToString(),
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                Name = profile.Name
            };
            return Page();
        }
    }
}
