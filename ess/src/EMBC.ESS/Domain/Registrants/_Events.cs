using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class RegistrantCreated : Event
    {
        public RegistrantCreated(string id, string name, string address, string identity, string dateOfBirth)
        {
            Id = id;
            Name = name;
            Address = address;
            Identity = identity;
            DateOfBirth = dateOfBirth;
        }

        public string Id { get; }
        public string Name { get; }
        public string Address { get; }
        public string Identity { get; }
        public string DateOfBirth { get; }
    }

    public class RegistrantNameChanged : Event
    {
        public RegistrantNameChanged(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }

    public class RegistrantAddressChanged : Event
    {
        public RegistrantAddressChanged(string id, string address)
        {
            Id = id;
            Address = address;
        }

        public string Id { get; }
        public string Address { get; }
    }

    public class RegistrantIdentifierAdded : Event
    {
        public RegistrantIdentifierAdded(string id, string identifierType, string identifierValue)
        {
            Id = id;
            IdentifierType = identifierType;
            IdentifierValue = identifierValue;
        }

        public string Id { get; }
        public string IdentifierType { get; }
        public string IdentifierValue { get; }
    }

    public class RegistrantIdentifierValueChanged : Event
    {
        public RegistrantIdentifierValueChanged(string id, string identifierType, string identifierNewValue)
        {
            Id = id;
            IdentifierType = identifierType;
            IdentifierNewValue = identifierNewValue;
        }

        public string Id { get; }
        public string IdentifierType { get; }
        public string IdentifierNewValue { get; }
    }

    public class RegistrantIdentifierRemoved : Event
    {
        public RegistrantIdentifierRemoved(string id, string identifierType)
        {
            Id = id;
            IdentifierType = identifierType;
        }

        public string Id { get; }
        public string IdentifierType { get; }
    }
}
