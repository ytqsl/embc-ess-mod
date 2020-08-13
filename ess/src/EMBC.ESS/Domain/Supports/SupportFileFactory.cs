using System;
using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public interface ISupportsFileFactory
    {
        Task<SupportsFile> CreateAsync(OpenSupportsFile cmd);
    }

    public class SupportFileFactory : ISupportsFileFactory
    {
        private readonly IProvideSequenceNumbers sequenceNumbersProvider;

        public SupportFileFactory(IProvideSequenceNumbers sequenceNumbersProvider)
        {
            this.sequenceNumbersProvider = sequenceNumbersProvider;
        }

        public async Task<SupportsFile> CreateAsync(OpenSupportsFile cmd)
        {
            //TODO: verify task is active
            var nextSequence = await sequenceNumbersProvider.NextAsync<SupportsFile>();
            var perliminaryAssessment = new NeedsAssessment(cmd.HasInsurance, cmd.MedicationRequirements, cmd.FoodRequired);
            foreach (var animal in cmd.Animals) { perliminaryAssessment.AddAnimal(animal.Type, animal.Quantity, animal.HasFoodSupplies); }
            foreach (var member in cmd.Members) { perliminaryAssessment.AddMember(member.Name, member.DateOfBirth); }

            var newFile = new SupportsFile(GetSupportFileReferenceNumber(nextSequence), cmd.UserId, cmd.TaskId, DateTime.Now, cmd.SourceAddress,
                cmd.RegistrantIds, perliminaryAssessment, cmd.SupportsRequestReferenceNumber);
            return newFile;
        }

        private static string GetSupportFileReferenceNumber(ulong sequence) => $"{sequence}";
    }
}
