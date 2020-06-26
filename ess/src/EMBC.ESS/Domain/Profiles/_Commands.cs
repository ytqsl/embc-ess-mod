using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Profiles
{
    public class CreateProfile : IRequest<Guid>
    {
        public CreateProfile(string name, string address, string dateOfBirth)
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
