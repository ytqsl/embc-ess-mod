using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Registrants
{
    public interface IRegistrantProfileReadModelRepository
    {
        Task AddAsync(RegistrantProfile profile);

        Task Update(RegistrantProfile profile);

        Task<IEnumerable<RegistrantProfile>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null);

        Task<RegistrantProfile> GetByIdAsync(Guid id);
    }

    public class RegistrantProfile
    {
        public Guid Id { get; set; }
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

        public async Task<RegistrantProfile> GetByIdAsync(Guid guid)
        {
            await Task.CompletedTask;
            var profile = profiles.SingleOrDefault(p => p.Id == guid);
            return profile;
        }
    }
}
