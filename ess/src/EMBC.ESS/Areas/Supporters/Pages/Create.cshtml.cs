using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class CreateModel : PageModel
    {
        private readonly ICommandSender bus;

        public class Command
        {
            [Display(Name = "Full Name"), Required]
            public string Name { get; set; }

            [Display(Name = "Date of Birth"), Required]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address"), Required]
            public string Address { get; set; }
        }

        public CreateModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Command Data { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var id = await bus.SendAsync(new RegisterNew(Data.Name, Data.Address, Data.DateOfBirth));
            return RedirectToPage("./View", new { id });
        }
    }
}
