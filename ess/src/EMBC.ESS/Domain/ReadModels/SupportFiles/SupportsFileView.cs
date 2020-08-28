using System;
using System.Collections.Generic;

namespace EMBC.ESS.Domain.ReadModels.SupportFiles
{
    public class SupportsFileView
    {
        public string ReferenceNumber { get; set; }
        public string SourceAddress { get; set; }
        public IEnumerable<string> Registrants { get; set; } = Array.Empty<string>();
        public NeedsAssessment PerliminaryNeedsAssessment { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class NeedsAssessment
    {
        public string TaskNumber { get; set; }
        public string ByUser { get; set; }
        public DateTime DateCompleted { get; set; }
        public bool RequiresFood { get; set; }
        public string MedicationRequirements { get; set; }
        public IEnumerable<Member> Members { get; set; } = Array.Empty<Member>();
        public IEnumerable<Animal> Animals { get; set; } = Array.Empty<Animal>();
        public bool HasInsurance { get; set; }
        public string SourceAddress { get; set; }
        public IEnumerable<Note> Notes { get; set; } = Array.Empty<Note>();
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

    public class Note
    {
        public string User { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
