using System;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportFileRegistrantByRegistrant : IQuery<SupportsFileRegistrantView>
    {
        public Guid RegistrantId { get; set; }
    }

    public class SupportFileSupporterByRegistrant : IQuery<SupportsFileSupporterView>
    {
        public Guid RegistrantId { get; set; }
    }
}
