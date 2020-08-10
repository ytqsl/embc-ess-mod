using System;
using System.Collections.Generic;

namespace EMBC.ESS.Domain.Supports
{
    public class SupportsFileRegistrantView
    {
        public string SourceAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdate { get; set; }
        public IEnumerable<Registrant> Registrants { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<Animal> Animals { get; set; }
        public NeedsAssessmentDetails NeedsAssessment { get; set; }

        public class Registrant
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class Member
        {
            public string Name { get; set; }
        }

        public class Animal
        {
            public string Name { get; set; }
            public bool RequireFood { get; set; }
        }

        public class NeedsAssessmentDetails
        {
            public bool? HasInsurance { get; set; }
            public string MedicationRequirements { get; set; }
            public bool? FoodRequired { get; set; }
        }
    }

    public class SupportsFileSupporterView
    {
        public string SourceAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdate { get; set; }
        public IEnumerable<Registrant> Registrants { get; set; }
        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<Animal> Animals { get; set; }
        public string DescriptionNote { get; set; }
        public string PlanNote { get; set; }
        public string RecommendationNote { get; set; }

        public class Registrant
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class Member
        {
            public string Name { get; set; }
        }

        public class Animal
        {
            public string Name { get; set; }
            public bool RequireFood { get; set; }
        }

        public class NeedsAssessmentDetails
        {
            public bool? HasInsurance { get; set; }
            public string MedicationRequirements { get; set; }
            public bool? FoodRequired { get; set; }
        }
    }

    public class SupportRequestView
    {
        public string ReferenceNumber { get; set; }
        public string Registrant { get; }
        public DateTime Time { get; }
        public string SourceAddress { get; }

        public IEnumerable<Member> Members { get; set; }
        public IEnumerable<Animal> Animals { get; set; }
        public bool? HasInsurance { get; set; }
        public string MedicationRequirements { get; set; }
        public bool? FoodRequired { get; set; }

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
