using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class CommandsHandler
    {
        private readonly IRepository<SupportsRequest> supportsRequestsRepository;
        private readonly IRepository<SupportsFile> supportsFilesRepository;
        private readonly IProvideSequenceNumbers sequenceNumberProvider;
        private readonly ISupportsFileFactory supportsFileFactory;

        public CommandsHandler(IRepository<SupportsRequest> supportsRequestsRepository, IRepository<SupportsFile> supportsFilesRepository,
            IProvideSequenceNumbers sequenceNumberProvider, ISupportsFileFactory supportsFileFactory)
        {
            this.supportsRequestsRepository = supportsRequestsRepository;
            this.supportsFilesRepository = supportsFilesRepository;
            this.sequenceNumberProvider = sequenceNumberProvider;
            this.supportsFileFactory = supportsFileFactory;
        }

        public async Task<string> Handle(RequestSupports cmd)
        {
            var nextSequence = await sequenceNumberProvider.NextAsync<SupportsFile>();
            var newRequest = SupportsRequest.Create(nextSequence, cmd);

            await supportsRequestsRepository.SaveAsync(newRequest);

            return newRequest.Id;
        }

        public async Task<string> Handle(CompleteNeedsAssessment cmd)
        {
            var supportsFile = await supportsFilesRepository.GetByIdAsync(cmd.SupportFileReferenceNumber);
            if (supportsFile == null)
            {
                supportsFile = await supportsFileFactory.CreateAsync(cmd);
            }

            var needsAssessment = NeedsAssessment.FromCommand(cmd);
            supportsFile.CompleteNeedsAssessment(cmd.UserId, cmd.TaskId, cmd.Time, needsAssessment);

            await supportsFilesRepository.SaveAsync(supportsFile);

            return supportsFile.Id;
        }
    }
}
