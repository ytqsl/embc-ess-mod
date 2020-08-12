using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class ViewModel : PageModel
    {
        private readonly IQuerySender bus;

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

        public ViewModel(IQuerySender bus)
        {
            this.bus = bus;
        }

        [ViewData]
        public ProfileViewModel Data { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var profile = await bus.QueryAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = id });
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
