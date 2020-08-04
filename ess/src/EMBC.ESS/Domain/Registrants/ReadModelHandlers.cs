using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Registrants
{
    public class ReadModelEventHandler
    {
        private readonly IRegistrantProfileReadModelRepository repository;

        public ReadModelEventHandler(IRegistrantProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(RegistrantCreated evt)
        {
            await repository.AddAsync(new RegistrantProfile
            {
                Id = evt.Id,
                Name = evt.Name,
                Address = evt.Address,
                DateOfBirth = evt.DateOfBirth
            });
        }

        public async Task Handle(RegistrantAddressChanged evt)
        {
            var profile = await repository.GetByIdAsync(evt.Id);
            profile.Address = evt.Address;
            await repository.Update(profile);
        }

        public async Task Handle(RegistrantIdentifierAdded evt)
        {
            var profile = await repository.GetByIdAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = evt.IdentifierValue;
            await repository.Update(profile);
        }

        public async Task Handle(RegistrantIdentifierValueChanged evt)
        {
            var profile = await repository.GetByIdAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = evt.IdentifierNewValue;
            await repository.Update(profile);
        }

        public async Task Handle(RegistrantIdentifierRemoved evt)
        {
            var profile = await repository.GetByIdAsync(evt.Id);
            if (evt.IdentifierType == "date_of_birth") profile.DateOfBirth = null;
            await repository.Update(profile);
        }

        public async Task Handle(RegistrantNameChanged evt)
        {
            var profile = await repository.GetByIdAsync(evt.Id);
            profile.Name = evt.Name;
            await repository.Update(profile);
        }
    }

    public class ReadModelQueryHandler
    {
        private readonly IRegistrantProfileReadModelRepository repository;

        public ReadModelQueryHandler(IRegistrantProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<RegistrantProfile>> Handle(ProfilesQuery query)
        {
            return await repository.GetAllAsync(query.FirstName, query.LastName, query.DateOfBirth);
        }

        public async Task<RegistrantProfile> Handle(ProfileByIdQuery query)
        {
            return await repository.GetByIdAsync(query.Id);
        }
    }
}
