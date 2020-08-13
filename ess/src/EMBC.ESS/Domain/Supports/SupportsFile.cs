using System;
using System.Collections.Generic;
using EMBC.ESS.Domain.Common;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsFile : AggregateRoot
    {
#pragma warning disable IDE0051 // Remove unused private members

        private void Apply(SupportsFileOpened evt)
        {
            Id = evt.ReferenceNumber;
        }

        private void Apply(NoteAddedToSupportFile evt)
        {
        }

#pragma warning restore IDE0051 // Remove unused private members

        public SupportsFile(string referenceNumber, string userId, string taskId, DateTime time, string sourceAddress, IEnumerable<string> registrants, NeedsAssessment perliminaryAssessment, string supportsRequestReferenceNumber)
        {
            ApplyChange(new SupportsFileOpened(referenceNumber, userId, time, taskId, sourceAddress, registrants, perliminaryAssessment, supportsRequestReferenceNumber));
        }

        public void AddNote(string userId, string type, string content, DateTime time)
        {
            ApplyChange(new NoteAddedToSupportFile(Id, userId, type, content, time));
        }
    }

    public enum SupportsFileStatus
    {
        Active,
        Closed
    }

    //public enum SupportRequestType
    //{
    //    Food,
    //    Lodging,
    //    Transportation,
    //    Clothing,
    //    Incidentals
    //}

    public class NeedsAssessment
    {
        private readonly List<Member> members = new List<Member>();
        private readonly List<Animal> animals = new List<Animal>();
        public IEnumerable<Member> Members => members;
        public IEnumerable<Animal> Animals => animals;

        public bool? HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool RequiresFood { get; }

        public NeedsAssessment(bool? hasInsurance, string medicationRequirements, bool requiresFood)
        {
            HasInsurance = hasInsurance;
            MedicationRequirements = medicationRequirements;
            RequiresFood = requiresFood;
        }

        public void AddMember(string name, string dateOfBirth)
        {
            members.Add(new Member { Name = name, DateOfBirth = dateOfBirth });
        }

        public void AddAnimal(string type, int quantity, bool hasSufficientFood)
        {
            animals.Add(new Animal { Type = type, Quantity = quantity, HasFoodSupplies = hasSufficientFood });
        }
    }

    public class Member
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
    }

    public class Animal
    {
        public string Type { get; set; }
        public bool HasFoodSupplies { get; set; }
        public int Quantity { get; set; }
    }

    public static class NoteTypes
    {
        public const string General = "general";
        public const string Affect = "affect";
        public const string Recovery = "recovery";
        public const string Reference = "reference";
    }

    public class Note
    {
        public string User { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }
}
