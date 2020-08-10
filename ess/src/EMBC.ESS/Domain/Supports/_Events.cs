using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsRequestReceived : Event
    {
        public SupportsRequestReceived(string referenceNumber, string registrantId, string sourceAddress, IEnumerable<Member> members, IEnumerable<Animal> animals,
            bool? hasInsurance, string medicationRequirements, bool? foodRequired)
        {
            ReferenceNumber = referenceNumber;
            RegistrantId = registrantId;
            SourceAddress = sourceAddress;
            Members = members;
            Animals = animals;
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            FoodRequired = foodRequired;
        }

        public string ReferenceNumber { get; }

        public string RegistrantId { get; }
        public string SourceAddress { get; }
        public IEnumerable<Member> Members { get; }
        public IEnumerable<Animal> Animals { get; }
        public bool? HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool? FoodRequired { get; }

        public class Member
        {
            public string DateOfBirth { get; set; }
            public string Name { get; set; }
        }

        public class Animal
        {
            public string Type { get; set; }
            public bool? HasFoodSupplies { get; set; }
        }
    }

    public class SupportsFileOpened : Event
    {
        public SupportsFileOpened(string referenceNumber, string openingUserId, DateTime time, string taskId, IEnumerable<string> registrants,
            NeedsAssessment perliminaryAssessment, string supportsRequestReferenceNumber)
        {
            OpeningUserId = openingUserId;
            Time = time;
            TaskId = taskId;
            Registrants = registrants;
            PerliminaryAssessment = perliminaryAssessment;
            SupportsRequestReferenceNumber = supportsRequestReferenceNumber;
            ReferenceNumber = referenceNumber;
        }

        public string OpeningUserId { get; }
        public DateTime Time { get; }
        public string TaskId { get; }
        public IEnumerable<string> Registrants { get; }
        public NeedsAssessment PerliminaryAssessment { get; }
        public string SupportsRequestReferenceNumber { get; }
        public string ReferenceNumber { get; }
    }

    public class NoteAddedToSupportFile : Event
    {
        public NoteAddedToSupportFile(string id, string userId, string type, string content, DateTime time)
        {
            Id = id;
            UserId = userId;
            Type = type;
            Content = content;
            Time = time;
        }

        public string Id { get; }
        public string UserId { get; }
        public string Type { get; }
        public string Content { get; }
        public DateTime Time { get; }
    }

    //public class NeedsAssessed : Event
    //{
    //    public NeedsAssessed(string id, string byUserId, string taskId, DateTime time, string sourceAddress,
    //        IEnumerable<Member> members, IEnumerable<Animal> animals, bool? hasInsurance, string medicationRequirements, bool? foodRequired, IEnumerable<Note> notes)
    //    {
    //        ByUserId = byUserId;
    //        TaskId = taskId;
    //        Time = time;
    //        Id = id;
    //        SourceAddress = sourceAddress;
    //        Members = members;
    //        Animals = animals;
    //        HasInsurance = hasInsurance;
    //        MedicationRequirements = medicationRequirements;
    //        FoodRequired = foodRequired;
    //        Notes = notes;
    //    }

    //    public string ByUserId { get; }
    //    public string TaskId { get; }
    //    public DateTime Time { get; }
    //    public string Id { get; }

    //    public IEnumerable<Guid> Registrants { get; }
    //    public string SourceAddress { get; }
    //    public IEnumerable<Member> Members { get; }
    //    public IEnumerable<Animal> Animals { get; }
    //    public bool? HasInsurance { get; }
    //    public string MedicationRequirements { get; }
    //    public bool? FoodRequired { get; }
    //    public IEnumerable<Note> Notes { get; }

    //    public class Member
    //    {
    //        public string DateOfBirth { get; set; }
    //        public string Name { get; set; }
    //    }

    //    public class Animal
    //    {
    //        public string Type { get; set; }
    //        public bool? HasFoodSupplies { get; set; }
    //    }

    //    public class Note
    //    {
    //        public string User { get; set; }
    //        public string Type { get; set; }
    //        public string Content { get; set; }
    //    }
    //}
}
