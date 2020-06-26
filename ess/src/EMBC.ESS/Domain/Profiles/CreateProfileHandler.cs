using System;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Profiles
{
    public class CreateProfileHandler
    {
        private readonly IRepository<Profile> repository;

        public CreateProfileHandler(IRepository<Profile> repository)
        {
            this.repository = repository;
        }

        public async Task<Guid> Handle(CreateProfile cmd)
        {
            var profile = new Profile(cmd.Name, cmd.Address, cmd.DateOfBirth);
            await repository.SaveAsync(profile);
            return profile.Id;
        }
    }
}
