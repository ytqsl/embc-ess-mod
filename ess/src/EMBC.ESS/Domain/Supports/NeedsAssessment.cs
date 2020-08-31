using System.Collections.Generic;

namespace EMBC.ESS.Domain.Supports
{
    public class NeedsAssessment
    {
        public static NeedsAssessment FromCommand(CompleteNeedsAssessment cmd)
        {
            var needsAssessment = new NeedsAssessment(cmd.HasInsurance, cmd.MedicationRequirements, cmd.FoodRequired);
            foreach (var animal in cmd.Animals) { needsAssessment.AddAnimal(animal.Type, animal.Quantity, animal.HasFoodSupplies); }
            foreach (var member in cmd.Members) { needsAssessment.AddMember(member.Name, member.DateOfBirth); }
            if (!string.IsNullOrEmpty(cmd.AffectOnRegistrantNote)) { needsAssessment.AddNote(NoteTypes.Affect, cmd.AffectOnRegistrantNote); }
            if (!string.IsNullOrEmpty(cmd.RecoveryPlanNote)) { needsAssessment.AddNote(NoteTypes.Recovery, cmd.RecoveryPlanNote); }
            if (!string.IsNullOrEmpty(cmd.ReferencesNote)) { needsAssessment.AddNote(NoteTypes.Reference, cmd.ReferencesNote); }

            return needsAssessment;
        }

        private readonly List<Member> members = new List<Member>();
        private readonly List<Animal> animals = new List<Animal>();
        private readonly List<Note> notes = new List<Note>();
        public IEnumerable<Member> Members => members;
        public IEnumerable<Animal> Animals => animals;
        public IEnumerable<Note> Notes => notes;

        public bool HasInsurance { get; }
        public string MedicationRequirements { get; }
        public bool RequiresFood { get; }

        public NeedsAssessment(bool hasInsurance, string medicationRequirements, bool requiresFood)
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

        public void AddNote(string type, string content)
        {
            notes.Add(new Note { Type = type, Content = content });
        }

        public struct Member
        {
            public string Name { get; set; }
            public string DateOfBirth { get; set; }
        }

        public struct Animal
        {
            public string Type { get; set; }
            public bool HasFoodSupplies { get; set; }
            public int Quantity { get; set; }
        }

        public struct Note
        {
            public string Type { get; set; }
            public string Content { get; set; }
        }
    }

    public static class NoteTypes
    {
        public const string General = "general";
        public const string Affect = "affect";
        public const string Recovery = "recovery";
        public const string Reference = "reference";
    }
}
