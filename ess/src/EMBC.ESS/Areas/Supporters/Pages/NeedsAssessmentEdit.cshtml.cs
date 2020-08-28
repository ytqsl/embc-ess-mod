using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using EMBC.ESS.Domain.ReadModels.SupportFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EMBC.ESS.Areas.Supporters.Pages
{
    public class NeedsAssessmentEditModel : PageModel
    {
        private readonly ICommandSender bus;

        public NeedsAssessmentEditModel(ICommandSender bus)
        {
            this.bus = bus;
        }

        public class SupportRequestView
        {
            public string ReferenceNumber { get; set; }

            [Display(Name = "Name"), Required]
            public string RegistrantName { get; set; }

            [Display(Name = "Home address"), Required]
            public string RegistrantHomeAddress { get; set; }

            [Display(Name = "Evacuated from"), Required]
            public string SourceAddress { get; set; }

            [Display(Name = "Household member name")]
            public string Member1Name { get; set; }

            [Display(Name = "Household member date of birth")]
            public string Member1DateOfBirth { get; set; }

            [Display(Name = "Animal type")]
            public string Animal1Type { get; set; }

            [Display(Name = "Has food for animals")]
            public bool? Animal1HasFoodSupplies { get; set; }

            [Display(Name = "Number of animals")]
            public int? Animal1Quantity { get; set; }

            [Display(Name = "Has insurance")]
            public bool HasInsurance { get; set; }

            [Display(Name = "Medication required")]
            public string MedicationRequirements { get; set; }

            [Display(Name = "Requires food")]
            public bool FoodRequired { get; set; }

            public string Status { get; set; }
            public DateTime RequestedOn { get; set; }
        }

        [BindProperty]
        public SupportRequestView Data { get; set; }

        public async Task<IActionResult> OnGet(string @ref)
        {
            var request = await bus.SendAsync(new SupportsFileByReferenceNumberQuery { ReferenceNumber = @ref });
            if (request == null) return NotFound();
            var profile = await bus.SendAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = request.Registrants.First() });
            Data = new SupportRequestView
            {
                ReferenceNumber = @ref,
                RegistrantName = profile.Name,
                RegistrantHomeAddress = profile.Address,
                SourceAddress = request.SourceAddress,
                Member1Name = request.PerliminaryNeedsAssessment.Members.FirstOrDefault()?.Name,
                Member1DateOfBirth = request.PerliminaryNeedsAssessment.Members.FirstOrDefault()?.DateOfBirth,
                Animal1Type = request.PerliminaryNeedsAssessment.Animals.FirstOrDefault()?.Type,
                Animal1Quantity = request.PerliminaryNeedsAssessment.Animals.FirstOrDefault()?.Quantity,
                Animal1HasFoodSupplies = request.PerliminaryNeedsAssessment.Animals.FirstOrDefault()?.HasFoodSupplies,
                FoodRequired = request.PerliminaryNeedsAssessment.RequiresFood,
                HasInsurance = request.PerliminaryNeedsAssessment.HasInsurance,
                MedicationRequirements = request.PerliminaryNeedsAssessment.MedicationRequirements,
                Status = request.Status,
                RequestedOn = request.CreatedOn
            };

            return Page();
        }
    }
}
