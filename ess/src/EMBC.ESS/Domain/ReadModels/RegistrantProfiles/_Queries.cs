using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.ReadModels.RegistrantProfiles
{
    public class RegistrantProfileByRegistrantIdQuery : ICommand<RegistrantProfileView>
    {
        public string RegistrantId { get; set; }
    }

    public class RegistrantProfilesByPersonalDetailsQuery : ICommand<IEnumerable<RegistrantProfileView>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
    }

    public class RegistrantProfileBySuppoerFileReferenceNumberQuery : ICommand<IEnumerable<RegistrantProfileView>>
    {
        public string ReferenceNumber { get; set; }
    }
}
