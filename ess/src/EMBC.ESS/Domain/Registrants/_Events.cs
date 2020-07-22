using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class RegistrantCreated : Event
    {
        public RegistrantCreated(Guid id, string name, string address, string identity, string dateOfBirth)
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
