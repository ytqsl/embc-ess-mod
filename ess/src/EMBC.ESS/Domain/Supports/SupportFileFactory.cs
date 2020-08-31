using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public interface ISupportsFileFactory
    {
        Task<SupportsFile> CreateAsync(CompleteNeedsAssessment cmd);
    }

    public class SupportFileFactory : ISupportsFileFactory
    {
        private readonly IProvideSequenceNumbers sequenceNumbersProvider;

        public SupportFileFactory(IProvideSequenceNumbers sequenceNumbersProvider)
        {
            this.sequenceNumbersProvider = sequenceNumbersProvider;
        }

        public async Task<SupportsFile> CreateAsync(CompleteNeedsAssessment cmd)
        {
            var nextSequence = await sequenceNumbersProvider.NextAsync<SupportsFile>();

            var newFile = new SupportsFile(GetSupportFileReferenceNumber(nextSequence), cmd.UserId, cmd.TaskId, cmd.Time, cmd.RegistrantId, cmd.SourceAddress);
            foreach (var registrant in cmd.RegistrantIds)
            {
                newFile.AssignRegistrant(new Registrant(registrant));
            }
            return newFile;
        }

        private static string GetSupportFileReferenceNumber(ulong sequence) => $"{sequence}";
    }
}
