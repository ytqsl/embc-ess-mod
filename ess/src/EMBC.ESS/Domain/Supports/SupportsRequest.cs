using System.Linq;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsRequest : AggregateRoot
    {
        /// <summary>
        /// Factory method for creating supports requests
        /// </summary>
        /// <param name="nextSequence"></param>
        /// <param name="cmd"></param>
        public static SupportsRequest Create(ulong nextSequence, RequestSupports cmd)
        {
            var newRequest = new SupportsRequest();
            newRequest.ApplyChange(new SupportsRequestReceived(GetSupportRequestReferenceNumber(nextSequence),
                cmd.Registrant,
                cmd.SourceAddress,
                cmd.Members.Select(m => new SupportsRequestReceived.Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                cmd.Animals.Select(a => new SupportsRequestReceived.Animal { Type = a.Type, HasFoodSupplies = a.HasFoodSupplies }),
                cmd.HasInsurance, cmd.MedicationRequirements, cmd.FoodRequired
                ));

            return newRequest;
        }

        private static string GetSupportRequestReferenceNumber(ulong sequence) => $"{sequence}";

#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(SupportsRequestReceived evt)
        {
        }

#pragma warning restore IDE0051 // Remove unused private members
    }
}
