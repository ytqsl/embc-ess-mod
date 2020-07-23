using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Registrants
{
    public class RegistrationDomainException : DomainException
    {
        public RegistrationDomainException(string message) : base(message)
        {
        }

        public RegistrationDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
