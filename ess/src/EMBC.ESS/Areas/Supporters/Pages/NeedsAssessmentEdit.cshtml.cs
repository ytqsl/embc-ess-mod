using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;
using EMBC.ESS.Domain.ReadModels.SupportFiles;
using EMBC.ESS.Domain.Supports;
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
            [Required]
            public string ReferenceNumber { get; set; }

            [Required]
            public string RegistrantId { get; set; }

            [Display(Name = "Name")]
            public string RegistrantName { get; set; }

            [Display(Name = "Home address")]
            public string RegistrantHomeAddress { get; set; }

            [Display(Name = "Evacuated from")]
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

            public IEnumerable<(DateTime when, string byUser, string note)> PreviousNotes { get; set; } = Array.Empty<(DateTime when, string byUser, string note)>();

            [Display(Name = "Assessment")]
            public string Note { get; set; }
        }

        [BindProperty]
        public SupportRequestView Data { get; set; }

        public async Task<IActionResult> OnGetAsync(string @ref, string regId)
        {
            var file = await bus.SendAsync(new SupportsFileByReferenceNumberQuery { ReferenceNumber = @ref });
            if (file == null) return NotFound();
            var profile = await bus.SendAsync(new RegistrantProfileByRegistrantIdQuery { RegistrantId = regId });
            var lastNeedsAssessment = file.NeedsAssessments.First();
            Data = new SupportRequestView
            {
                ReferenceNumber = @ref,
                RegistrantName = profile.Name,
                RegistrantId = profile.Id,
                RegistrantHomeAddress = profile.Address,
                SourceAddress = file.SourceAddress,
                Member1Name = lastNeedsAssessment.Members.FirstOrDefault()?.Name,
                Member1DateOfBirth = lastNeedsAssessment.Members.FirstOrDefault()?.DateOfBirth,
                Animal1Type = lastNeedsAssessment.Animals.FirstOrDefault()?.Type,
                Animal1Quantity = lastNeedsAssessment.Animals.FirstOrDefault()?.Quantity,
                Animal1HasFoodSupplies = lastNeedsAssessment.Animals.FirstOrDefault()?.HasFoodSupplies,
                FoodRequired = lastNeedsAssessment.RequiresFood,
                HasInsurance = lastNeedsAssessment.HasInsurance,
                MedicationRequirements = lastNeedsAssessment.MedicationRequirements,
                Status = file.Status,
                RequestedOn = file.CreatedOn,
                PreviousNotes = file.NeedsAssessments
                    .Where(na => na.Notes.Any())
                    .Select(na => (na.DateCompleted, na.ByUser, note: na.Notes.First().Content))
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var members = string.IsNullOrEmpty(Data.Member1Name) ?
                Array.Empty<CompleteNeedsAssessment.Member>() :
                new[] { new CompleteNeedsAssessment.Member { Name = Data.Member1Name, DateOfBirth = Data.Member1DateOfBirth } };

            var animals = string.IsNullOrEmpty(Data.Animal1Type)
                ? Array.Empty<CompleteNeedsAssessment.Animal>()
                : new[] { new CompleteNeedsAssessment.Animal { Type = Data.Animal1Type, Quantity = Data.Animal1Quantity.Value, HasFoodSupplies = Data.Animal1HasFoodSupplies.Value } };

            await bus.SendAsync(new CompleteNeedsAssessment(Data.ReferenceNumber, "a user", DateTime.Now, "a task", Data.RegistrantId, Data.ReferenceNumber, Data.SourceAddress,
                members, animals,
                Data.HasInsurance, Data.MedicationRequirements, Data.FoodRequired, null, null, Data.Note));

            return RedirectToPage("RegistrantView", new { Id = Data.RegistrantId });
        }
    }
}
