using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class RequestSupports : ICommand<string>
    {
        public RequestSupports(string registrant, DateTime time, string sourceAddress, IEnumerable<Member> members, IEnumerable<Animal> animals,
            bool hasInsurance, string medicationRequirements, bool foodRequired)
        {
            Registrant = registrant;
            Time = time;
            SourceAddress = sourceAddress;
            Members = members;
            Animals = animals;
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            FoodRequired = foodRequired;
        }

        public string Registrant { get; }
        public DateTime Time { get; }
        public string SourceAddress { get; }

        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<Animal> Animals { get; set; }
        public bool HasInsurance { get; set; }
        public string MedicationRequirements { get; set; }
        public bool FoodRequired { get; set; }

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

    public class CompleteNeedsAssessment : ICommand
    {
        public CompleteNeedsAssessment(string supportFileReferenceNumber, string userId, DateTime time, string taskId, string registrantId, string supportsRequestReferenceNumber, string sourceAddress, IEnumerable<Member> members, IEnumerable<Animal> animals,
            bool hasInsurance, string medicationRequirements, bool foodRequired, string recoveryPlanNote, string affectOnRegistrantNote, string referencesNote)
        {
            SupportFileReferenceNumber = supportFileReferenceNumber;
            UserId = userId;
            Time = time;
            TaskId = taskId;
            SupportsRequestReferenceNumber = supportsRequestReferenceNumber;
            SourceAddress = sourceAddress;
            Members = members;
            Animals = animals;
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            FoodRequired = foodRequired;
            RecoveryPlanNote = recoveryPlanNote;
            AffectOnRegistrantNote = affectOnRegistrantNote;
            ReferencesNote = referencesNote;
            RegistrantId = registrantId;
        }

        public string SupportFileReferenceNumber { get; }
        public string UserId { get; }
        public DateTime Time { get; }
        public string TaskId { get; }
        public string SupportsRequestReferenceNumber { get; }
        public string RegistrantId { get; }
        public IEnumerable<string> OtherRegistrants { get; }
        public string SourceAddress { get; }

        public IEnumerable<Member> Members { get; }
        public IEnumerable<Animal> Animals { get; }
        public bool HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool FoodRequired { get; }
        public string RecoveryPlanNote { get; }
        public string AffectOnRegistrantNote { get; }
        public string ReferencesNote { get; }

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
}
