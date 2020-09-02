using System;
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
                NeedsAssessments = new[] {
                    new NeedsAssessment
                    {
                        ByUser = null,
                        TaskNumber = null,
                        DateCompleted = evt.Time,
                        MedicationRequirements = evt.MedicationRequirements,
                        RequiresFood = evt.FoodRequired,
                        Animals = evt.Animals.Select(a => new Animal { Type = a.Type, Quantity = a.Quantity, HasFoodSupplies = a.HasFoodSupplies }),
                        Members = evt.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                        HasInsurance = evt.HasInsurance,
                        Notes  = Array.Empty<Note>()
                    }
                }
            };
            await repository.SetAsync(evt.ReferenceNumber, file);
        }

        public async Task HandleAsync(SupportsFileOpened evt)
        {
            var supportRequest = await repository.GetByKeyAsync(evt.ReferenceNumber);
            var file = new SupportsFileView
            {
                Status = "Open",
                ReferenceNumber = evt.ReferenceNumber,
                SourceAddress = evt.SourceAddress,
                CreatedOn = evt.Time,
                UpdatedOn = evt.Time,
                NeedsAssessments = supportRequest == null
                    ? Array.Empty<NeedsAssessment>()
                    : supportRequest.NeedsAssessments.ToArray()
            };

            await repository.SetAsync(evt.ReferenceNumber, file);
        }

        public async Task HandleAsync(NeedsAssessmentCompleted evt)
        {
            var file = await repository.GetByKeyAsync(evt.ReferenceNumber);
            file.Status = "Reviewed";
            file.NeedsAssessments = file.NeedsAssessments.Append(new NeedsAssessment
            {
                ByUser = evt.ByUserId,
                TaskNumber = evt.TaskId,
                DateCompleted = evt.Time,
                MedicationRequirements = evt.MedicationRequirements,
                RequiresFood = evt.FoodRequired,
                Animals = evt.Animals.Select(a => new Animal { Type = a.Type, Quantity = a.Quantity, HasFoodSupplies = a.HasFoodSupplies }),
                Members = evt.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                HasInsurance = evt.HasInsurance,
                Notes = evt.Notes.Select(n => new Note { Type = n.Type, Content = n.Content })
            });
            await repository.SetAsync(evt.ReferenceNumber, file);
        }

        public async Task HandleAsync(RegistrantAddedToSupportsFile evt)
        {
            var file = await repository.GetByKeyAsync(evt.ReferenceNumber);
            file.Registrants.Append(evt.RegistrantId);
            await repository.SetAsync(evt.ReferenceNumber, file);
        }
    }
}
