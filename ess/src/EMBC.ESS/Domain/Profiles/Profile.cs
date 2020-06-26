using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Profiles
{
    public class Profile : AggregateRoot
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string DateOfBirth { get; private set; }
        public string Identity { get; private set; }

#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(ProfileCreated evt)
        {
            Id = evt.Id;
            Name = evt.Name;
            Address = evt.Address;
            DateOfBirth = evt.DateOfBirth;
            Identity = evt.Identity;
        }

#pragma warning restore IDE0051 // Remove unused private members

        public Profile()
        {
        }

        public Profile(string name, string address, string dateOfBirth)
        {
            var evt = new ProfileCreated(Guid.NewGuid(), name, address, string.Empty, dateOfBirth);
            ApplyChange(evt);
        }
    }
}
