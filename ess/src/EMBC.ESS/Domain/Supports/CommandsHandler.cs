using System;
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

        public async Task<string> Handle(OpenSupportsFile cmd)
        {
            var newFile = await supportsFileFactory.CreateAsync(cmd);

            if (!string.IsNullOrEmpty(cmd.RecoveryPlanNote)) { newFile.AddNote(cmd.UserId, NoteTypes.Recovery, cmd.RecoveryPlanNote, DateTime.Now); }
            if (!string.IsNullOrEmpty(cmd.AffectOnRegistrantNote)) { newFile.AddNote(cmd.UserId, NoteTypes.Affect, cmd.AffectOnRegistrantNote, DateTime.Now); }
            if (!string.IsNullOrEmpty(cmd.ReferencesNote)) { newFile.AddNote(cmd.UserId, NoteTypes.Reference, cmd.ReferencesNote, DateTime.Now); }

            await supportsFilesRepository.SaveAsync(newFile);

            return newFile.Id;
        }
    }
}
