using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class RegisterNew : ICommand<Guid>
    {
        public RegisterNew(string name, string address, string dateOfBirth)
        {
            Name = name;
            Address = address;
            DateOfBirth = dateOfBirth;
        }

        public string Name { get; }
        public string Address { get; }
        public string DateOfBirth { get; }
    }

    public class UpdateDetails : ICommand
    {
        public UpdateDetails(Guid id, string name, string address)
        {
            Id = id;
            Name = name;
            Address = address;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Address { get; }
    }

    public class AddIdentifier : ICommand
    {
        public AddIdentifier(Guid id, string identifierValue, string identifierType)
        {
            Id = id;
            IdentifierValue = identifierValue;
            IdentifierType = identifierType;
        }

        public Guid Id { get; }
        public string IdentifierValue { get; }
        public string IdentifierType { get; }
    }

    public class ReplaceIdentifier : ICommand
    {
        public ReplaceIdentifier(Guid id, string identifierNewValue, string identifierType)
        {
            Id = id;
            IdentifierNewValue = identifierNewValue;
            IdentifierType = identifierType;
        }

        public Guid Id { get; }
        public string IdentifierNewValue { get; }
        public string IdentifierType { get; }
    }

    public class RemoveIdentifier : ICommand
    {
        public RemoveIdentifier(Guid id, string identifierType)
        {
            Id = id;
            IdentifierType = identifierType;
        }

        public Guid Id { get; }
        public string IdentifierType { get; }
    }
}
