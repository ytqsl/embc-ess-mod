using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class ViewModel : PageModel
    {
        public class ProfileViewModel
        {
            public string Id { get; set; }

            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address")]
            public string Address { get; set; }

            public string Status { get; set; }
        }

        private readonly ICommandSender bus;

        public ViewModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        [ViewData]
        public ProfileViewModel Profile { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var profile = await bus.SendAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = id });
            if (profile == null) return NotFound();
            Profile = new ProfileViewModel
            {
                Id = id,
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                Name = profile.Name,
                Status = profile.Status
            };

            return Page();
        }
    }
}
