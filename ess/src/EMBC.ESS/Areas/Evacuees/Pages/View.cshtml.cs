using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class ViewModel : PageModel
    {
        public class ProfileViewModel
        {
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address")]
            public string Address { get; set; }
        }

        private readonly IQuerySender bus;

        public ViewModel(IQuerySender bus)
        {
            this.bus = bus;
        }

        [ViewData]
        public ProfileViewModel Profile { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var profile = await bus.QueryAsync(new ProfileByIdQuery(id));
            Profile = new ProfileViewModel
            {
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                Name = profile.Name
            };
            return Page();
        }
    }
}
