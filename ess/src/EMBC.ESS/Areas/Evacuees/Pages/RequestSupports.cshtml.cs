using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Supports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Evacuees.Pages
{
    public class RequestSupportsModel : PageModel
    {
        private readonly ICommandSender bus;

        public RequestSupportsModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        public class ViewModel
        {
            [Required]
            public string RegistrantId { get; set; }

            [Display(Name = "Where are you evacuated from?"), Required]
            public string SourceAddress { get; set; }

            [Display(Name = "Household member name")]
            public string Member1Name { get; set; }

            [Display(Name = "Household member date of birth")]
            public string Member1DateOfBirth { get; set; }

            [Display(Name = "Animal type")]
            public string Animal1Type { get; set; }

            [Display(Name = "Do you have enough food for the animal(s)?")]
            public bool Animal1HasFoodSupplies { get; set; }

            [Display(Name = "How many animals?")]
            public int? Animal1Quantity { get; set; }

            [Display(Name = "Do you have insurance?")]
            public bool HasInsurance { get; set; }

            [Display(Name = "Do you require any medications?")]
            public string MedicationRequirements { get; set; }

            [Display(Name = "Do you require food?")]
            public bool FoodRequired { get; set; }
        }

        [BindProperty]
        public ViewModel Data { get; set; }

        public IActionResult OnGet(string id)
        {
            Data = new ViewModel { RegistrantId = id };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var members = Data.Member1Name != null
                ? new[] { new RequestSupports.Member { Name = Data.Member1Name, DateOfBirth = Data.Member1DateOfBirth } }
                : Array.Empty<RequestSupports.Member>();
            var animals = Data.Animal1Quantity.HasValue
                ? new[] { new RequestSupports.Animal { Type = Data.Animal1Type, Quantity = Data.Animal1Quantity.Value, HasFoodSupplies = Data.Animal1HasFoodSupplies } }
                : Array.Empty<RequestSupports.Animal>();

            var cmd = new RequestSupports(Data.RegistrantId, DateTime.Now, Data.SourceAddress,
                members, animals, Data.HasInsurance, Data.MedicationRequirements, Data.FoodRequired);

            var referenceNumber = await bus.SendAsync(cmd);
            return RedirectToPage("view", new { id = Data.RegistrantId });
        }
    }
}
