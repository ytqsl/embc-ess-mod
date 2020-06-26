using System;
using System.Collections.Generic;
using EMBC.ESS.Areas.Supporters.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class IndexModel : PageModel
    {
        [ViewData]
        public IEnumerable<Profile> Profiles { get; set; }

        public IActionResult OnGet(string firstName, string lastName, string dateOfBirth)
        {
            Profiles = new[]
           {
                new Profile{Name = "1", Address="2",DateOfBirth="3", Id=Guid.NewGuid().ToString()},
                new Profile{Name = $"{firstName} {lastName}", DateOfBirth = dateOfBirth, Address = "address", Id=Guid.NewGuid().ToString()}
            }; return Page();
        }
    }
}
