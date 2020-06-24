using EMBC.ESS.Areas.Evacuees.Models;
using EMBC.ESS.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public Profile Profile { get; set; }

        [ViewData]
        public Profile ProfileView => Profile;

        public IActionResult OnGet()
        {
            if (!TempData.ContainsKey("profile"))
            {
                return RedirectToPage("Register");
            }
            Profile = TempData.Get<Profile>("profile");
            TempData.Keep("profile");
            return Page();
        }
    }
}
