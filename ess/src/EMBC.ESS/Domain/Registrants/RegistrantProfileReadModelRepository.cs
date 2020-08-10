using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public interface IRegistrantProfileReadModelRepository
    {
        Task AddAsync(RegistrantProfile profile);

        Task Update(RegistrantProfile profile);

        Task<IEnumerable<RegistrantProfile>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null);

        Task<RegistrantProfile> GetByIdAsync(string id);
    }

    public class RegistrantProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
    }

    public class RegistrantProfileReadModelRepository : IRegistrantProfileReadModelRepository
    {
        private static readonly List<RegistrantProfile> profiles = new List<RegistrantProfile>();

        public async Task AddAsync(RegistrantProfile profile)
        {
            await Task.CompletedTask;
            profiles.Add(profile);
        }

        public async Task Update(RegistrantProfile profile)
        {
            await Task.CompletedTask;
            //Update is done in memory by the handler, in the future there would be a call to persist the changes
        }

        public async Task<IEnumerable<RegistrantProfile>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null)
        {
            await Task.CompletedTask;
            return profiles.Where(p => (string.IsNullOrWhiteSpace(firstName) || p.Name.StartsWith(firstName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(lastName) || p.Name.EndsWith(lastName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(dateOfBirth) || p.DateOfBirth.Equals(dateOfBirth, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<RegistrantProfile> GetByIdAsync(string guid)
        {
            await Task.CompletedTask;
            var profile = profiles.SingleOrDefault(p => p.Id == guid);
            return profile;
        }
    }

    public class RegistrantProfileReadModelBuilder
    {
        private readonly IRegistrantProfileReadModelRepository repository;
        private readonly IReadModelRepository<Registration> esRepository;

        public RegistrantProfileReadModelBuilder(IRegistrantProfileReadModelRepository repository, IReadModelRepository<Registration> esRepository)
        {
            this.repository = repository;
            this.esRepository = esRepository;
        }

        public async Task BuildAsync()
        {
            await foreach (var r in esRepository.GetAsync())
            {
                await repository.AddAsync(new RegistrantProfile
                {
                    Id = r.Id,
                    Address = r.Address,
                    DateOfBirth = r.Idenfifiers.FirstOrDefault(i => i.Key == "date_of_birth").Value,
                    Name = r.Name
                });
            }
        }
    }

    public class ESRegistrationProfileReadModel : IRegistrantProfileReadModelRepository
    {
        private readonly IReadModelRepository<Registration> readModelRepository;

        public ESRegistrationProfileReadModel(IReadModelRepository<Registration> readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        public Task AddAsync(RegistrantProfile profile)
        {
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<RegistrantProfile>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null)
        {
            await Task.CompletedTask;
            return readModelRepository.GetAsync(
                a =>
                (firstName == null || a.Name.StartsWith(firstName)) &&
                (lastName == null || a.Name.EndsWith(lastName)) &&
                (dateOfBirth == null || !a.Idenfifiers.Any(i => i.Key == "date_of_birth") || a.Idenfifiers.First(i => i.Key == "date_of_birth").Value.Equals(dateOfBirth))
            ).Select(ToProfile).ToEnumerable().ToArray();
        }

        public async Task<RegistrantProfile> GetByIdAsync(string id)
        {
            var registration = await readModelRepository.GetByIdAsync(id);
            if (registration == null) return null;
            return ToProfile(registration);
        }

        public Task Update(RegistrantProfile profile)
        {
            return Task.CompletedTask;
        }

        private RegistrantProfile ToProfile(Registration r) => new RegistrantProfile
        {
            Id = r.Id,
            Address = r.Address,
            DateOfBirth = r.Idenfifiers.FirstOrDefault(i => i.Key == "date_of_birth").Value,
            Name = r.Name
        };
    }
}
