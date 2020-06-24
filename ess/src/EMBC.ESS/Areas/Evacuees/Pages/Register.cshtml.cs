using System.Threading.Tasks;
using EMBC.ESS.Areas.Evacuees.Models;
using EMBC.ESS.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class RegisterModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            await Task.CompletedTask;
            return Page();
        }

        [BindProperty]
        public Profile Profile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await Task.CompletedTask;
            TempData.Put("profile", Profile);
            return RedirectToPage("./Index");
        }
    }
}
