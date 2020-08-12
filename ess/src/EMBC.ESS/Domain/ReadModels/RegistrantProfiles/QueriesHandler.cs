using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;
using EMBC.ESS.Domain.ReadModels.RegistrantProfiles;

namespace EMBC.ESS.Domain.ReadModels
{
    public class QueriesHandler
    {
        private readonly IReadModelRepository<RegistrantProfileView> repository;

        public QueriesHandler(IReadModelRepository<RegistrantProfileView> repository)
        {
            this.repository = repository;
        }

        public async Task<RegistrantProfileView> Handle(RegistrantProfileByRegistrantIdQuery query)
        {
            return await repository.GetByKeyAsync(query.RegistrantId);
        }

        public async Task<IEnumerable<RegistrantProfileView>> Handle(RegistrantProfilesByPersonalDetailsQuery query)
        {
            var firstName = query.FirstName;
            var lastName = query.LastName;
            var dateOfBirth = query.DateOfBirth;

            bool filter(RegistrantProfileView p) => (string.IsNullOrWhiteSpace(firstName) || p.Name.StartsWith(firstName, StringComparison.OrdinalIgnoreCase)) &&
               (string.IsNullOrWhiteSpace(lastName) || p.Name.EndsWith(lastName, StringComparison.OrdinalIgnoreCase)) &&
               (string.IsNullOrWhiteSpace(dateOfBirth) || p.DateOfBirth.Equals(dateOfBirth, StringComparison.OrdinalIgnoreCase));

            return await repository.GetAsync(filter).ToArrayAsync();
        }

        public async Task<IEnumerable<RegistrantProfileView>> Handle(RegistrantProfileBySuppoerFileReferenceNumberQuery query)
        {
            return await repository.GetAsync(p => p.Files.Any(f => f.ReferenceNumber == query.ReferenceNumber)).ToArrayAsync();
        }
    }
}
