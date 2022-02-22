﻿// -------------------------------------------------------------------------
//  Copyright © 2021 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  https://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

using System;
using EMBC.Registrants.API.Controllers;

namespace EMBC.Registrants.API.Mappers
{
    public class Mappings : AutoMapper.Profile
    {
        public Mappings()
        {
            CreateMap<Controllers.Profile, ESS.Shared.Contracts.Events.RegistrantProfile>()
                .ForMember(d => d.Id, opts => opts.Ignore())
                .ForMember(d => d.AuthenticatedUser, opts => opts.Ignore())
                .ForMember(d => d.VerifiedUser, opts => opts.Ignore())
                .ForMember(d => d.UserId, opts => opts.MapFrom(s => s.Id))
                .ForMember(d => d.DateOfBirth, opts => opts.MapFrom(s => s.PersonalDetails.DateOfBirth))
                .ForMember(d => d.FirstName, opts => opts.MapFrom(s => s.PersonalDetails.FirstName))
                .ForMember(d => d.LastName, opts => opts.MapFrom(s => s.PersonalDetails.LastName))
                .ForMember(d => d.Gender, opts => opts.MapFrom(s => s.PersonalDetails.Gender))
                .ForMember(d => d.Initials, opts => opts.MapFrom(s => s.PersonalDetails.Initials))
                .ForMember(d => d.PreferredName, opts => opts.MapFrom(s => s.PersonalDetails.PreferredName))
                .ForMember(d => d.Email, opts => opts.MapFrom(s => s.ContactDetails.Email))
                .ForMember(d => d.Phone, opts => opts.MapFrom(s => s.ContactDetails.Phone))
                .ForMember(d => d.SecurityQuestions, opts => opts.MapFrom(s => s.SecurityQuestions))
                .ForMember(d => d.CreatedOn, opts => opts.Ignore())
                .ForMember(d => d.LastModified, opts => opts.Ignore())
                .ForMember(d => d.CreatedByDisplayName, opts => opts.Ignore())
                .ForMember(d => d.CreatedByUserId, opts => opts.Ignore())
                .ForMember(d => d.LastModifiedDisplayName, opts => opts.Ignore())
                .ForMember(d => d.LastModifiedUserId, opts => opts.Ignore())

                .ReverseMap()

                .ForMember(d => d.IsMailingAddressSameAsPrimaryAddress, opts => opts.MapFrom(s =>
                    string.Equals(s.MailingAddress.Country, s.PrimaryAddress.Country, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.StateProvince, s.PrimaryAddress.StateProvince, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.Community, s.PrimaryAddress.Community, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.City, s.PrimaryAddress.City, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.PostalCode, s.PrimaryAddress.PostalCode, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.AddressLine1, s.PrimaryAddress.AddressLine1, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(s.MailingAddress.AddressLine2, s.PrimaryAddress.AddressLine2, StringComparison.InvariantCultureIgnoreCase)))
                ;

            CreateMap<SecurityQuestion, ESS.Shared.Contracts.Events.SecurityQuestion>()
                .ReverseMap()
                ;

            CreateMap<Address, ESS.Shared.Contracts.Events.Address>()
                .ReverseMap()
                ;

            CreateMap<NeedsAssessment, ESS.Shared.Contracts.Events.NeedsAssessment>()
                .ForMember(d => d.CompletedOn, opts => opts.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Notes, opts => opts.Ignore())
                .ForMember(d => d.RecommendedReferralServices, opts => opts.Ignore())
                .ForMember(d => d.HaveMedicalSupplies, opts => opts.Ignore())
                .ForMember(d => d.CanProvideFood, opts => opts.MapFrom(s => s.CanEvacueeProvideFood))
                .ForMember(d => d.CanProvideLodging, opts => opts.MapFrom(s => s.CanEvacueeProvideLodging))
                .ForMember(d => d.CanProvideClothing, opts => opts.MapFrom(s => s.CanEvacueeProvideClothing))
                .ForMember(d => d.CanProvideTransportation, opts => opts.MapFrom(s => s.CanEvacueeProvideTransportation))
                .ForMember(d => d.CanProvideIncidentals, opts => opts.MapFrom(s => s.CanEvacueeProvideIncidentals))
                .ForMember(d => d.TakeMedication, opts => opts.MapFrom(s => s.HaveMedication))
                .ForMember(d => d.HavePetsFood, opts => opts.MapFrom(s => s.HasPetsFood))
                .ForMember(d => d.CompletedBy, opts => opts.Ignore())
             ;

            CreateMap<ESS.Shared.Contracts.Events.NeedsAssessment, NeedsAssessment>()
                .ForMember(d => d.CanEvacueeProvideFood, opts => opts.MapFrom(s => s.CanProvideFood))
                .ForMember(d => d.CanEvacueeProvideLodging, opts => opts.MapFrom(s => s.CanProvideLodging))
                .ForMember(d => d.CanEvacueeProvideClothing, opts => opts.MapFrom(s => s.CanProvideClothing))
                .ForMember(d => d.CanEvacueeProvideTransportation, opts => opts.MapFrom(s => s.CanProvideTransportation))
                .ForMember(d => d.CanEvacueeProvideIncidentals, opts => opts.MapFrom(s => s.CanProvideIncidentals))
                .ForMember(d => d.HaveMedication, opts => opts.MapFrom(s => s.TakeMedication))
                .ForMember(d => d.HasPetsFood, opts => opts.MapFrom(s => s.HavePetsFood))
             ;

            CreateMap<HouseholdMember, ESS.Shared.Contracts.Events.HouseholdMember>()
                .ForMember(d => d.DateOfBirth, opts => opts.MapFrom(s => s.Details.DateOfBirth))
                .ForMember(d => d.FirstName, opts => opts.MapFrom(s => s.Details.FirstName))
                .ForMember(d => d.LastName, opts => opts.MapFrom(s => s.Details.LastName))
                .ForMember(d => d.Gender, opts => opts.MapFrom(s => s.Details.Gender))
                .ForMember(d => d.Initials, opts => opts.MapFrom(s => s.Details.Initials))
                .ForMember(d => d.IsPrimaryRegistrant, opts => opts.MapFrom(s => s.IsPrimaryRegistrant))
                .ForMember(d => d.LinkedRegistrantId, opts => opts.Ignore())
                .ForMember(d => d.RestrictedAccess, opts => opts.Ignore())
                .ForMember(d => d.Verified, opts => opts.Ignore())
                .ForMember(d => d.Authenticated, opts => opts.Ignore())
                ;

            CreateMap<ESS.Shared.Contracts.Events.HouseholdMember, HouseholdMember>()
                .ForPath(d => d.Details.FirstName, opts => opts.MapFrom(s => s.FirstName))
                .ForPath(d => d.Details.LastName, opts => opts.MapFrom(s => s.LastName))
                .ForPath(d => d.Details.Gender, opts => opts.MapFrom(s => s.Gender))
                .ForPath(d => d.Details.Initials, opts => opts.MapFrom(s => s.Initials))
                .ForPath(d => d.Details.DateOfBirth, opts => opts.MapFrom(s => s.DateOfBirth))
                ;

            CreateMap<Pet, ESS.Shared.Contracts.Events.Pet>()
                .ReverseMap()
                ;

            CreateMap<EvacuationFile, ESS.Shared.Contracts.Events.EvacuationFile>()
                .ForMember(d => d.Id, opts => opts.MapFrom(s => s.FileId))
                .ForMember(d => d.RelatedTask, opts => opts.Ignore())
                .ForMember(d => d.CreatedOn, opts => opts.Ignore())
                .ForMember(d => d.EvacuationDate, opts => opts.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.RestrictedAccess, opts => opts.Ignore())
                .ForMember(d => d.PrimaryRegistrantId, opts => opts.Ignore())
                .ForMember(d => d.SecurityPhraseChanged, opts => opts.MapFrom(s => s.SecretPhraseEdited))
                .ForMember(d => d.SecurityPhrase, opts => opts.MapFrom(s => s.SecretPhrase))
                .ForMember(d => d.RegistrationLocation, opts => opts.Ignore())
                .ForMember(d => d.Status, opts => opts.MapFrom(s => EvacuationFileStatus.Pending))
                .ForMember(d => d.HouseholdMembers, opts => opts.Ignore())
                .ForMember(d => d.Notes, opts => opts.Ignore())
                .ForMember(d => d.Supports, opts => opts.Ignore())
                ;

            CreateMap<ESS.Shared.Contracts.Events.EvacuationFile, EvacuationFile>()
                .ForMember(d => d.FileId, opts => opts.MapFrom(s => s.Id))
                .ForMember(d => d.EvacuationFileDate, opts => opts.MapFrom(s => s.EvacuationDate))
                .ForMember(d => d.IsRestricted, opts => opts.MapFrom(s => s.RestrictedAccess))
                .ForMember(d => d.SecretPhrase, opts => opts.MapFrom(s => s.SecurityPhrase))
                .ForMember(d => d.SecretPhraseEdited, opts => opts.MapFrom(s => false))
                .ForMember(d => d.LastModified, opts => opts.Ignore())
                ;

            CreateMap<ESS.Shared.Contracts.Events.Support, Support>()
                .IncludeAllDerived()
                .ForMember(d => d.IssuingMemberTeamName, opts => opts.MapFrom(s => s.IssuedBy.TeamName))
                ;

            CreateMap<ESS.Shared.Contracts.Events.Referral, Referral>()
                .IncludeAllDerived()
                .ForMember(d => d.SupplierId, opts => opts.MapFrom(s => s.SupplierDetails != null ? s.SupplierDetails.Id : null))
                .ForMember(d => d.SupplierName, opts => opts.MapFrom(s => s.SupplierDetails != null ? s.SupplierDetails.Name : null))
                .ForMember(d => d.SupplierAddress, opts => opts.MapFrom(s => s.SupplierDetails != null ? s.SupplierDetails.Address : null))
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.ClothingReferral, ClothingReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.IncidentalsReferral, IncidentalsReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.FoodGroceriesReferral, FoodGroceriesReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.FoodRestaurantReferral, FoodRestaurantReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.LodgingBilletingReferral, LodgingBilletingReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.LodgingGroupReferral, LodgingGroupReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.LodgingHotelReferral, LodgingHotelReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.TransportationOtherReferral, TransportationOtherReferral>()
                ;

            CreateMap<EMBC.ESS.Shared.Contracts.Events.TransportationTaxiReferral, TransportationTaxiReferral>()
                ;
        }
    }
}