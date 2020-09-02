using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsFile : AggregateRoot
    {
        private List<string> registrants = new List<string>();
        private string sourceAddress;
#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(SupportsFileOpened evt)
        {
            Id = evt.ReferenceNumber;
            sourceAddress = evt.SourceAddress;
        }

        private void Apply(RegistrantAddedToSupportsFile evt)
        {
            registrants.Add(evt.RegistrantId);
        }

        private void Apply(NeedsAssessmentCompleted evt)
        {
        }

#pragma warning restore IDE0051 // Remove unused private members

        public SupportsFile()
        {
        }

        public SupportsFile(string referenceNumber, string userId, string taskId, DateTime time, string registrantId, string sourceAddress, bool isSelfRegistration)
        {
            ApplyChange(new SupportsFileOpened(referenceNumber, userId, time, taskId, registrantId, sourceAddress, isSelfRegistration ? referenceNumber : null));
        }

        public void AssignRegistrant(Registrant registrant)
        {
            if (registrants.Contains(registrant.Id)) return;
            ApplyChange(new RegistrantAddedToSupportsFile(Id, registrant.Id));
        }

        public void CompleteNeedsAssessment(string userId, string taskId, DateTime completionDate, NeedsAssessment needsAssessment)
        {
            ApplyChange(NeedsAssessmentCompleted.FromNeedsAssessment(Id, userId, taskId, completionDate, needsAssessment));
        }
    }
}
