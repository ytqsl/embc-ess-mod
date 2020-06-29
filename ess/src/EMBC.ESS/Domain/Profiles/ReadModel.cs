using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMBC.ESS.Domain.Profiles
{
    public class ReadModelHandler
    {
        private readonly IProfileReadModelRepository repository;

        public ReadModelHandler(IProfileReadModelRepository repository)
        {
            this.repository = repository;
        }

        public async Task Handle(ProfileCreated evt)
        {
            await repository.AddAsync(new ProfileReadModel
            {
                Id = evt.Id,
                Name = evt.Name,
                Address = evt.Address,
                DateOfBirth = evt.DateOfBirth
            });
        }
    }

    public interface IProfileReadModelRepository
    {
        Task AddAsync(ProfileReadModel id);

        Task<IEnumerable<ProfileReadModel>> GetAllAsync();
    }

    public class ProfileReadModelRepository : IProfileReadModelRepository
    {
        private static readonly List<ProfileReadModel> profiles = new List<ProfileReadModel>();

        public async Task AddAsync(ProfileReadModel profile)
        {
            await Task.CompletedTask;
            profiles.Add(profile);
        }

        public async Task<IEnumerable<ProfileReadModel>> GetAllAsync()
        {
            await Task.CompletedTask;
            return profiles;
        }
    }

    public class ProfileReadModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
    }
}
