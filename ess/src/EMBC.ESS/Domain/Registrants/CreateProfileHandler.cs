using System;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class CreateProfileHandler
    {
        private readonly IRepository<Registration> repository;

        public CreateProfileHandler(IRepository<Registration> repository)
        {
            this.repository = repository;
        }

        public async Task<Guid> Handle(RegisterNew cmd)
        {
            var profile = new Registration(cmd.Name, cmd.Address, cmd.DateOfBirth);
            await repository.SaveAsync(profile);
            return profile.Id;
        }
    }
}
