using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.Registrants;

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
    }
}
