using System.Threading.Tasks;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.ReadModels.SupportFiles
{
    public class QueriesHandler
    {
        private readonly IReadModelRepository<SupportsFileView> repository;

        public QueriesHandler(IReadModelRepository<SupportsFileView> repository)
        {
            this.repository = repository;
        }

        public async Task<SupportsFileView> Handle(SupportsFileByReferenceNumberQuery query)
        {
            return await repository.GetByKeyAsync(query.ReferenceNumber);
        }
    }
}
