using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class RegistrantViewModel : PageModel
    {
        private readonly ICommandSender bus;

        public class ProfileViewModel
        {
            public string Id { get; set; }

            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Display(Name = "Date of Birth")]
            public string DateOfBirth { get; set; }

            [Display(Name = "home Address")]
            public string Address { get; set; }

            public string Status { get; set; }
        }

        public class SupportsRequestViewModel
        {
            public string ReferenceNumber { get; set; }
            public string From { get; set; }
            public DateTime CreatedOn { get; set; }
            public string Status { get; set; }
        }

        public class SupportsFileViewModel
        {
            public string ReferenceNumber { get; set; }
            public string From { get; set; }
            public DateTime CreatedOn { get; set; }
            public string Status { get; set; }
        }

        public RegistrantViewModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        [ViewData]
        public ProfileViewModel Profile { get; set; }

        [ViewData]
        public IEnumerable<SupportsRequestViewModel> PendingRequests { get; set; }

        [ViewData]
        public IEnumerable<SupportsFileViewModel> Files { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var profile = await bus.SendAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = id });
            Profile = new ProfileViewModel
            {
                Id = profile.Id.ToString(),
                Address = profile.Address,
                DateOfBirth = profile.DateOfBirth,
                Name = profile.Name,
                Status = profile.Status
            };
            PendingRequests = profile.PendingRequests.Select(r => new SupportsRequestViewModel
            {
                CreatedOn = r.RequestedOn,
                From = r.SourceAddress,
                ReferenceNumber = r.ReferenceNumber,
                Status = r.Status
            });

            Files = profile.Files.Select(f => new SupportsFileViewModel
            {
                CreatedOn = f.CreatedOn,
                Status = f.Status,
                ReferenceNumber = f.ReferenceNumber,
                From = f.SourceAddress,
            });

            return Page();
        }
    }
}
