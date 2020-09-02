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
            var isSelfRegistration = cmd.SupportsRequestReferenceNumber != null;
            var referenceNumber = !isSelfRegistration
                ? GetSupportFileReferenceNumber(await sequenceNumbersProvider.NextAsync<SupportsFile>())
                : cmd.SupportsRequestReferenceNumber;

            var newFile = new SupportsFile(referenceNumber, cmd.UserId, cmd.TaskId, cmd.Time, cmd.RegistrantId, cmd.SourceAddress, isSelfRegistration);

            return newFile;
        }

        private static string GetSupportFileReferenceNumber(ulong sequence) => $"{sequence}";
    }
}
