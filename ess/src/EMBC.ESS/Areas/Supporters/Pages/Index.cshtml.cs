using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IQuerySender bus;

        public IndexModel(IQuerySender bus)
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
        }

        [ViewData]
        public IEnumerable<ProfileViewModel> Profiles { get; set; }

        public async Task<IActionResult> OnGetAsync(string firstName, string lastName, string dateOfBirth)
        {
            var profiles = await bus.QueryAsync(new ProfilesQuery(firstName, lastName, dateOfBirth));

            Profiles = profiles.Select(p => new ProfileViewModel
            {
                Id = p.Id.ToString(),
                DateOfBirth = p.DateOfBirth,
                Address = p.Address,
                Name = p.Name
            }); ;
            return Page();
        }
    }
}
