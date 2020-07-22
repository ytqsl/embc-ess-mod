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
}
