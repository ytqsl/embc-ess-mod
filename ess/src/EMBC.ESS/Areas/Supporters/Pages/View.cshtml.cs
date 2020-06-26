using EMBC.ESS.Areas.Supporters.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class ViewModel : PageModel
    {
        [ViewData]
        public Profile ProfileView { get; set; }

        public IActionResult OnGet(string id)
        {
            ProfileView = new Profile { Id = id, Name = "1", Address = "2", DateOfBirth = "3", Identity = "123" };
            return Page();
        }
    }
}
