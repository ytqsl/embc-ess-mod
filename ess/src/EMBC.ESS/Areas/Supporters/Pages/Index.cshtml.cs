using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICommandSender bus;

        public IndexModel(ICommandSender bus)
        {
            this.bus = bus;
        }

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

        [ViewData]
        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public async Task<IActionResult> OnGetAsync(string firstName, string lastName, string dateOfBirth)
        {
            var profiles = await bus.SendAsync(new RegistrantProfilesByPersonalDetailsQuery { DateOfBirth = dateOfBirth, FirstName = firstName, LastName = lastName });

            Profiles = profiles.Select(p => new ProfileViewModel
            {
                Id = p.Id.ToString(),
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                Name = p.Name,
                Status = p.Status
            }); ;
            return Page();
        }
    }
}
