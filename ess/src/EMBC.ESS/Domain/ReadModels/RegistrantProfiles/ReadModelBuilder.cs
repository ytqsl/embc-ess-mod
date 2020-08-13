using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;
using EMBC.ESS.Domain.Supports;

namespace EMBC.ESS.Domain.ReadModels.RegistrantProfiles
{
    public class ReadModelBuilder
    {
        private readonly IReadModelRepository<RegistrantProfileView> repository;

        public ReadModelBuilder(IReadModelRepository<RegistrantProfileView> repository)
        {
            this.repository = repository;
        }

        public async Task HandleAsync(RegistrantCreated evt)
        {
            var profile = new RegistrantProfileView
            {
                Id = evt.Id,
                Name = evt.Name,
                Address = evt.Address,
                DateOfBirth = evt.DateOfBirth
            };
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(RegistrantAddressChanged evt)
        {
            var profile = await repository.GetByKeyAsync(evt.Id);
            profile.Address = evt.Address;
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(RegistrantIdentifierAdded evt)
        {
            var profile = await repository.GetByKeyAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = evt.IdentifierValue;
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(RegistrantIdentifierValueChanged evt)
        {
            var profile = await repository.GetByKeyAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = evt.IdentifierNewValue;
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(RegistrantIdentifierRemoved evt)
        {
            var profile = await repository.GetByKeyAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = null;
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(RegistrantNameChanged evt)
        {
            var profile = await repository.GetByKeyAsync(evt.Id);
            profile.Name = evt.Name;
            await repository.SetAsync(evt.Id, profile);
        }

        public async Task HandleAsync(SupportsRequestReceived evt)
        {
            var profile = await repository.GetByKeyAsync(evt.ReferenceNumber);
            profile.PendingRequests.Add(new SupportsRequestView
            {
                ReferenceNumber = evt.ReferenceNumber,
                SourceAddress = evt.SourceAddress,
                PerliminaryNeedsAssessment = new NeedsAssessment
                {
                    DateCompleted = evt.Time,
                    RequiresFood = evt.FoodRequired,
                    MedicationRequirements = evt.MedicationRequirements,
                    Animals = evt.Animals.Select(a => new Animal { Type = a.Type, HasFoodSupplies = a.HasFoodSupplies, Quantity = a.Quantity }).ToArray(),
                    Members = evt.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }).ToArray()
                },
            });
            await repository.SetAsync(evt.ReferenceNumber, profile);
        }

        public async Task HandleAsync(SupportsFileOpened evt)
        {
            var profile = await repository.GetByKeyAsync(evt.ReferenceNumber);
            profile.Files.Add(new SupportsFileView
            {
                ReferenceNumber = evt.ReferenceNumber,
                SourceAddress = evt.SourceAddress,
                PerliminaryNeedsAssessment = new NeedsAssessment
                {
                    DateCompleted = evt.Time,
                    RequiresFood = evt.PerliminaryAssessment.RequiresFood,
                    MedicationRequirements = evt.PerliminaryAssessment.MedicationRequirements,
                    Animals = evt.PerliminaryAssessment.Animals.Select(a => new Animal { Type = a.Type, HasFoodSupplies = a.HasFoodSupplies, Quantity = a.Quantity }).ToArray(),
                    Members = evt.PerliminaryAssessment.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }).ToArray()
                },
            });
            await repository.SetAsync(evt.ReferenceNumber, profile);
        }
    }
}
