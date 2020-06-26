using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Profiles
{
    public class ProfileCreated : Event
    {
        public ProfileCreated(Guid id, string name, string address, string identity, string dateOfBirth)
        {
            Id = id;
            Name = name;
            Address = address;
            Identity = identity;
            DateOfBirth = dateOfBirth;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Address { get; }
        public string Identity { get; }
        public string DateOfBirth { get; }
    }
}
