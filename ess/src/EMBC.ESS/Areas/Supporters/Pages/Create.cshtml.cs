using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class CreateModel : PageModel
    {
        public class CreateProfileCommand
        {
            [Display(Name = "Full Name"), Required]
            public string Name { get; set; }

            [Display(Name = "Date of Birth"), Required]
            public string DateOfBirth { get; set; }

            [Display(Name = "Home Address"), Required]
            public string Address { get; set; }

            [Display(Name = "BC Services Card Number")]
            public string Identity { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await Task.CompletedTask;
            return Page();
        }

        [BindProperty]
        public CreateProfileCommand Command { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var id = Guid.NewGuid();
            await Task.CompletedTask;
            TempData.Put("profile", Command);
            return RedirectToPage("./View", new { id });
        }
    }
}
