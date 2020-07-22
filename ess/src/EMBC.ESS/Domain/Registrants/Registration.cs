using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class Registration : AggregateRoot
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string DateOfBirth { get; private set; }
        public string Identity { get; private set; }

#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(RegistrantCreated evt)
        {
            Id = evt.Id;
            Name = evt.Name;
            Address = evt.Address;
            DateOfBirth = evt.DateOfBirth;
            Identity = evt.Identity;
        }

#pragma warning restore IDE0051 // Remove unused private members

        public Registration()
        {
        }

        public Registration(string name, string address, string dateOfBirth)
        {
            var evt = new RegistrantCreated(Guid.NewGuid(), name, address, string.Empty, dateOfBirth);
            ApplyChange(evt);
        }
    }
}
