using System;
using System.Collections.Generic;

namespace EMBC.ESS.Domain.ReadModels
{
    public class RegistrantProfileView
    {
        public string Checkpoint { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string DateOfBirth { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<SupportsFileView> Files { get; set; } = new List<SupportsFileView>();
        public List<SupportsRequestView> PendingRequests { get; set; } = new List<SupportsRequestView>();
        public string Status { get; set; }
    }

    public class SupportsFileView
    {
        public string ReferenceNumber { get; set; }
        public string SourceAddress { get; set; }
        public NeedsAssessment PerliminaryNeedsAssessment { get; set; }
    }

    public class SupportsRequestView
    {
        public string ReferenceNumber { get; set; }
        public string SourceAddress { get; set; }
        public NeedsAssessment PerliminaryNeedsAssessment { get; set; }
    }

    public class Member
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
    }

    public class Animal
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
        public bool HasFoodSupplies { get; set; }
    }

    public class NeedsAssessment
    {
        public DateTime DateCompleted { get; set; }
        public bool RequiresFood { get; set; }
        public string MedicationRequirements { get; set; }
        public IEnumerable<Member> Members { get; set; }

        public IEnumerable<Animal> Animals { get; set; }
    }
}
