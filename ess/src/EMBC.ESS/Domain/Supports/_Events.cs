using System;
using System.Collections.Generic;
using System.Linq;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsRequestReceived : Event
    {
        public SupportsRequestReceived(string referenceNumber, DateTime time, string registrantId, string sourceAddress, bool hasInsurance,
            string medicationRequirements, bool foodRequired, IEnumerable<Member> members, IEnumerable<Animal> animals)
        {
            ReferenceNumber = referenceNumber;
            RegistrantId = registrantId;
            SourceAddress = sourceAddress;
            Members = members;
            Animals = animals;
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            FoodRequired = foodRequired;
            Time = time;
        }

        public string ReferenceNumber { get; }

        public string RegistrantId { get; }
        public string SourceAddress { get; }
        public IEnumerable<Member> Members { get; }
        public IEnumerable<Animal> Animals { get; }
        public bool HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool FoodRequired { get; }
        public DateTime Time { get; }

        public class Member
        {
            public string DateOfBirth { get; set; }
            public string Name { get; set; }
        }

        public class Animal
        {
            public string Type { get; set; }
            public bool HasFoodSupplies { get; set; }
            public int Quantity { get; set; }
        }
    }

    public class SupportsFileOpened : Event
    {
        public SupportsFileOpened(string referenceNumber, string byUserId, DateTime time, string taskId, string registrantId, string sourceAddress)
        {
            OpeningUserId = byUserId;
            Time = time;
            TaskNumber = taskId;
            RegistrantId = registrantId;
            SourceAddress = sourceAddress;
            ReferenceNumber = referenceNumber;
        }

        public string OpeningUserId { get; }
        public DateTime Time { get; }
        public string TaskNumber { get; }
        public string RegistrantId { get; }
        public string SourceAddress { get; }
        public string ReferenceNumber { get; }
    }

    public class RegistrantAddedToSupportsFile : Event
    {
        public RegistrantAddedToSupportsFile(string referenceNumber, string id)
        {
            ReferenceNumber = referenceNumber;
            Id = id;
        }

        public string ReferenceNumber { get; }
        public string Id { get; }
    }

    public class NeedsAssessmentCompleted : Event
    {
        public static NeedsAssessmentCompleted FromNeedsAssessment(string referenceNumber, string byUserId, string taskId, DateTime time, NeedsAssessment needsAssessment)
        {
            return new NeedsAssessmentCompleted(referenceNumber, byUserId, taskId, time, needsAssessment.HasInsurance, needsAssessment.MedicationRequirements, needsAssessment.RequiresFood,
                needsAssessment.Members.Select(m => new Member { Name = m.Name, DateOfBirth = m.DateOfBirth }),
                needsAssessment.Animals.Select(a => new Animal { Type = a.Type, Quantity = a.Quantity, HasFoodSupplies = a.HasFoodSupplies }),
                needsAssessment.Notes.Select(n => new Note { Type = n.Type, Content = n.Content }));
        }

        public NeedsAssessmentCompleted(string referenceNumber, string byUserId, string taskId, DateTime time, bool hasInsurance, string medicationRequirements, bool foodRequired,
            IEnumerable<Member> members, IEnumerable<Animal> animals, IEnumerable<Note> notes)
        {
            ByUserId = byUserId;
            TaskId = taskId;
            Time = time;
            ReferenceNumber = referenceNumber;
            Members = members;
            Animals = animals;
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            FoodRequired = foodRequired;
            Notes = notes;
        }

        public string ByUserId { get; }
        public string TaskId { get; }
        public DateTime Time { get; }
        public string ReferenceNumber { get; }

        public IEnumerable<string> Registrants { get; }
        public IEnumerable<Member> Members { get; }
        public IEnumerable<Animal> Animals { get; }
        public bool HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool FoodRequired { get; }
        public IEnumerable<Note> Notes { get; }

        public class Member
        {
            public string DateOfBirth { get; set; }
            public string Name { get; set; }
        }

        public class Animal
        {
            public string Type { get; set; }
            public int Quantity { get; set; }
            public bool HasFoodSupplies { get; set; }
        }

        public class Note
        {
            public string Type { get; set; }
            public string Content { get; set; }
        }
    }
}
