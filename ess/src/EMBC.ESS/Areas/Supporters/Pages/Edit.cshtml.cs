using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using EMBC.ESS.Domain.Registrants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class EditModel : PageModel
    {
        private readonly ICommandSender bus;

        public class Command
        {
            [Required]
            public string Id { get; set; }

            [Display(Name = "Full Name"), Required]
            public string Name { get; set; }

            [Display(Name = "Date of Birth"), Required, ReadOnly(true)]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address"), Required]
            public string Address { get; set; }
        }

        public EditModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var profile = await bus.SendAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = id });
            Data = new Command
            {
                Id = profile.Id,
                Name = profile.Name,
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth
            };
            return Page();
        }

        [BindProperty]
        [ViewData]
        public Command Data { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await bus.SendAsync(new UpdateDetails(Data.Id, Data.Name, Data.Address));
            return RedirectToPage("./View", new { Data.Id });
        }
    }
}
