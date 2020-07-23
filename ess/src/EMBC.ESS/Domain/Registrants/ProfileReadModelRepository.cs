using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Registrants
{
    public interface IProfileReadModelRepository
    {
        Task AddAsync(RegistrantProfileView profile);

        Task Update(RegistrantProfileView profile);

        Task<IEnumerable<RegistrantProfileView>> GetAllAsync(string firstName = null, string lastName = null, string dateOfBirth = null);

        Task<RegistrantProfileView> GetByIdAsync(Guid id);
    }

    public class RegistrantProfileView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
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
}
