using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class Registration : AggregateRoot
    {
        public string Name { get; private set; }
        public string Address { get; private set; }

        public IEnumerable<KeyValuePair<string, string>> Idenfifiers => identifiers;

        private Dictionary<string, string> identifiers = new Dictionary<string, string>();

#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(RegistrantCreated evt)
        {
            Id = evt.Id;
            Name = evt.Name;
            Address = evt.Address;
        }

        private void Apply(RegistrantNameChanged evt)
        {
            Name = evt.Name;
        }

        private void Apply(RegistrantAddressChanged evt)
        {
            Address = evt.Address;
        }

        private void Apply(RegistrantIdentifierAdded evt)
        {
            identifiers.Add(evt.IdentifierType, evt.IdentifierValue);
        }

        private void Apply(RegistrantIdentifierRemoved evt)
        {
            identifiers.Remove(evt.IdentifierType);
        }

        private void Apply(RegistrantIdentifierValueChanged evt)
        {
            identifiers[evt.IdentifierType] = evt.IdentifierNewValue;
        }

#pragma warning restore IDE0051 // Remove unused private members

        public Registration()
        {
        }

        public Registration(string name, string address, string dateOfBirth)
        {
            ApplyChange(new RegistrantCreated(Guid.NewGuid(), name, address, string.Empty, dateOfBirth));
            ApplyChange(new RegistrantIdentifierAdded(Id, "date_of_birth", dateOfBirth));
        }

        public void UpdateName(string name)
        {
            if (Name == name) return;
            var evt = new RegistrantNameChanged(Id, name);
            ApplyChange(evt);
        }

        public void UpdateAddress(string address)
        {
            if (Address == address) return;
            var evt = new RegistrantAddressChanged(Id, address);
            ApplyChange(evt);
        }

        public void AddIdentifier(string identifierType, string identifierValue)
        {
            if (!identifiers.ContainsKey(identifierType)) throw new RegistrationDomainException($"Registrant {Id} already has identifier of type {identifierType}");
            var evt = new RegistrantIdentifierAdded(Id, identifierType, identifierValue);
            ApplyChange(evt);
        }

        public void ReplaceIdentifier(string identifierType, string identifierNewValue)
        {
            var evt = new RegistrantIdentifierValueChanged(Id, identifierType, identifierNewValue);
            ApplyChange(evt);
        }

        public void RemoveIdentifier(string identifierType)
        {
            if (!identifiers.ContainsKey(identifierType)) return;
            var evt = new RegistrantIdentifierRemoved(Id, identifierType);
            ApplyChange(evt);
        }
    }
}
