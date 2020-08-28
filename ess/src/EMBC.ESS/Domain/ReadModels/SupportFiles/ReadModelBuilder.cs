using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Supports;

namespace EMBC.ESS.Domain.ReadModels.SupportFiles
{
    public class ReadModelBuilder
    {
        private readonly IReadModelRepository<SupportsFileView> repository;

        public ReadModelBuilder(IReadModelRepository<SupportsFileView> repository)
        {
            this.repository = repository;
        }

        public async Task HandleAsync(SupportsRequestReceived evt)
        {
            var file = new SupportsFileView
            {
                ReferenceNumber = evt.ReferenceNumber,
                SourceAddress = evt.SourceAddress,
                CreatedOn = evt.Time,
                Status = "Pending",
                UpdatedOn = evt.Time,
                Registrants = new[] { evt.RegistrantId },
                PerliminaryNeedsAssessment = new NeedsAssessment
                {
                    ByUser = null,
                    TaskNumber = null,
                    DateCompleted = evt.Time,
                    MedicationRequirements = evt.MedicationRequirements,
                    RequiresFood = evt.FoodRequired,
                    Animals = evt.Animals.Select(a => new Animal { Type = a.Type, Quantity = a.Quantity, HasFoodSupplies = a.HasFoodSupplies }),
                    Members = evt.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                    HasInsurance = evt.HasInsurance,
                    SourceAddress = evt.SourceAddress,
                }
            };
            await repository.SetAsync(evt.ReferenceNumber, file);
        }

        public async Task HandleAsync(SupportsFileOpened evt)
        {
            // var request = await repository.GetByKeyAsync(evt.SupportsRequestReferenceNumber);
            var file = new SupportsFileView
            {
                ReferenceNumber = evt.ReferenceNumber,
                SourceAddress = evt.SourceAddress,
                CreatedOn = evt.Time,
                UpdatedOn = evt.Time,
                Registrants = evt.Registrants.ToArray(),
                PerliminaryNeedsAssessment = new NeedsAssessment
                {
                    ByUser = evt.OpeningUserId,
                    TaskNumber = evt.TaskNumber,
                    DateCompleted = evt.Time,
                    MedicationRequirements = evt.PerliminaryAssessment.MedicationRequirements,
                    RequiresFood = evt.PerliminaryAssessment.RequiresFood,
                    Animals = evt.PerliminaryAssessment.Animals.Select(a => new Animal { Type = a.Type, Quantity = a.Quantity, HasFoodSupplies = a.HasFoodSupplies }),
                    Members = evt.PerliminaryAssessment.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                    HasInsurance = evt.PerliminaryAssessment.HasInsurance,
                    SourceAddress = evt.SourceAddress,
                }
            };
            await repository.SetAsync(evt.ReferenceNumber, file);
        }
    }
}
