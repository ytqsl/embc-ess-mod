using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class ProfileByIdQuery : IQuery<RegistrantProfile>
    {
        public ProfileByIdQuery(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }

    public class ProfilesQuery : IQuery<IEnumerable<RegistrantProfile>>
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
}
