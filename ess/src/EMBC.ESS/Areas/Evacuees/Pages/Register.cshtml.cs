using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class RegisterModel : PageModel
    {
        public class RegistrationView
        {
            [Display(Name = "Full Name"), Required]
            public string Name { get; set; }

            [Display(Name = "Date of Birth"), Required]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address"), Required]
            public string Address { get; set; }
        }

        private readonly ICommandSender bus;

        public RegisterModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await Task.CompletedTask;
            return Page();
        }

        [BindProperty]
        public RegistrationView Profile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var id = await bus.SendAsync((IRequest<System.Guid>)new CreateProfile(Profile.Name, Profile.Address, Profile.DateOfBirth));
            return RedirectToPage("./View", new { id });
        }
    }
}
