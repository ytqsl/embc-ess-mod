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

    public class RegistrantNameChanged : Event
    {
        public RegistrantNameChanged(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }
    }

    public class RegistrantAddressChanged : Event
    {
        public RegistrantAddressChanged(Guid id, string address)
        {
            Id = id;
            Address = address;
        }

        public Guid Id { get; }
        public string Address { get; }
    }

    public class RegistrantIdentifierAdded : Event
    {
        public RegistrantIdentifierAdded(Guid id, string identifierType, string identifierValue)
        {
            Id = id;
            IdentifierType = identifierType;
            IdentifierValue = identifierValue;
        }

        public Guid Id { get; }
        public string IdentifierType { get; }
        public string IdentifierValue { get; }
    }

    public class RegistrantIdentifierValueChanged : Event
    {
        public RegistrantIdentifierValueChanged(Guid id, string identifierType, string identifierNewValue)
        {
            Id = id;
            IdentifierType = identifierType;
            IdentifierNewValue = identifierNewValue;
        }

        public Guid Id { get; }
        public string IdentifierType { get; }
        public string IdentifierNewValue { get; }
    }

    public class RegistrantIdentifierRemoved : Event
    {
        public RegistrantIdentifierRemoved(Guid id, string identifierType)
        {
            Id = id;
            IdentifierType = identifierType;
        }

        public Guid Id { get; }
        public string IdentifierType { get; }
    }
}
