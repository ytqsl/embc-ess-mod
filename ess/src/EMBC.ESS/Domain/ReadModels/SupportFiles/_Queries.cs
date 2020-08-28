using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.ReadModels.SupportFiles
{
    public class SupportsFileByReferenceNumberQuery : ICommand<SupportsFileView>
    {
        public string ReferenceNumber { get; set; }
    }
}
