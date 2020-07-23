using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
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
}
