using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class ReadModelEventHandler
    {
        private readonly IProfileReadModelRepository repository;

        public ReadModelEventHandler(IProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(RegistrantCreated evt)
        {
            await repository.AddAsync(new RegistrantProfileView
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

    public class ProfileByIdQuery : IQuery<RegistrantProfileView>
    {
        public ProfileByIdQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class ProfilesQuery : IQuery<IEnumerable<RegistrantProfileView>>
    {
        public ProfilesQuery(string firstName, string lastName, string dateOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string DateOfBirth { get; }
    }

    public class ReadModelQueryHandler
    {
        private readonly IProfileReadModelRepository repository;

        public ReadModelQueryHandler(IProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<RegistrantProfileView>> Handle(ProfilesQuery query)
        {
            return await repository.GetAllAsync(query.FirstName, query.LastName, query.DateOfBirth);
        }

        public async Task<RegistrantProfileView> Handle(ProfileByIdQuery query)
        {
            return await repository.GetByIdAsync(query.Id);
        }
    }

    public interface IProfileReadModelRepository
    {
        Task AddAsync(RegistrantProfileView profile);

        Task Update(RegistrantProfileView profile);

        Task<IEnumerable<RegistrantProfileView>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null);

        Task<RegistrantProfileView> GetByIdAsync(Guid id);
    }

    public class ProfileReadModelRepository : IProfileReadModelRepository
    {
        private static readonly List<RegistrantProfileView> profiles = new List<RegistrantProfileView>();

        public async Task AddAsync(RegistrantProfileView profile)
        {
            await Task.CompletedTask;
            profiles.Add(profile);
        }

        public async Task Update(RegistrantProfileView profile)
        {
            await Task.CompletedTask;
            //Update is done in memory by the handler, in the future there would be a call to persist the changes
        }

        public async Task<IEnumerable<RegistrantProfileView>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null)
        {
            await Task.CompletedTask;
            return profiles.Where(p => (string.IsNullOrWhiteSpace(firstName) || p.Name.StartsWith(firstName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(lastName) || p.Name.EndsWith(lastName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(dateOfBirth) || p.DateOfBirth.Equals(dateOfBirth, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<RegistrantProfileView> GetByIdAsync(Guid guid)
        {
            await Task.CompletedTask;
            var profile = profiles.SingleOrDefault(p => p.Id == guid);
            return profile;
        }
    }

    public class RegistrantProfileView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
    }
}
