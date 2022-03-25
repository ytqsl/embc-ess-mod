﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EMBC.ESS.Utilities.Dynamics;
using EMBC.ESS.Utilities.Dynamics.Microsoft.Dynamics.CRM;
using Microsoft.OData.Client;

namespace EMBC.ESS.Resources.Evacuees
{
    public class EvacueesRepository : IEvacueesRepository, IInvitationRepository
    {
        private readonly EssContext essContext;
        private readonly IEssContextFactory essContextFactory;
        private readonly IMapper mapper;

        public EvacueesRepository(IEssContextFactory essContextFactory, IMapper mapper)
        {
            essContext = essContextFactory.Create();
            this.essContextFactory = essContextFactory;
            this.mapper = mapper;
        }

        public async Task<ManageEvacueeCommandResult> Manage(ManageEvacueeCommand cmd) =>
            cmd switch
            {
                SaveEvacuee command => await Handle(command),
                DeleteEvacuee command => await Handle(command),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };

        public async Task<EvacueeQueryResult> Query(EvacueeQuery query) =>
            query switch
            {
                EvacueeQuery q => await Handle(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };

        public async Task<ManageInvitationCommandResult> Manage(ManageInvitationCommand cmd) =>
            cmd switch
            {
                CreateNewEmailInvitation command => await Handle(command),
                CompleteInvitation command => await Handle(command),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };

        public async Task<InvitationQueryResult> Query(InvitationQuery query) =>
            query switch
            {
                EmailInvitationQuery q => await Handle(q),
                _ => throw new NotSupportedException($"{query.GetType().Name} is not supported")
            };

        private async Task<ManageEvacueeCommandResult> Handle(SaveEvacuee cmd)
        {
            if (cmd.Evacuee.SecurityQuestions.Count() > 3) throw new ArgumentException($"Registrant can have a max of 3 Security Questions");
            var contact = mapper.Map<contact>(cmd.Evacuee);
            var existingContact = contact.contactid.HasValue
                ? essContext.contacts
                    .Expand(c => c.era_Country)
                    .Expand(c => c.era_MailingCountry)
                    .Expand(c => c.era_ProvinceState)
                    .Expand(c => c.era_MailingProvinceState)
                    .Where(c => c.contactid == contact.contactid.Value).SingleOrDefault()
                : GetContactIdForBcscId(contact.era_bcservicescardid);

            essContext.DetachAll();

            if (existingContact == null)
            {
                contact.contactid = Guid.NewGuid();
                essContext.AddTocontacts(contact);
            }
            else
            {
                contact.contactid = existingContact.contactid;
                essContext.AttachTo(nameof(EssContext.contacts), contact);
                essContext.UpdateObject(contact);
            }

            essContext.SetLink(contact, nameof(contact.era_Country), essContext.LookupCountryByCode(cmd.Evacuee.PrimaryAddress.Country));
            essContext.SetLink(contact, nameof(contact.era_ProvinceState), essContext.LookupStateProvinceByCode(cmd.Evacuee.PrimaryAddress.StateProvince));
            essContext.SetLink(contact, nameof(contact.era_City), essContext.LookupJurisdictionByCode(cmd.Evacuee.PrimaryAddress.Community));

            essContext.SetLink(contact, nameof(contact.era_MailingCountry), essContext.LookupCountryByCode(cmd.Evacuee.MailingAddress.Country));
            essContext.SetLink(contact, nameof(contact.era_MailingProvinceState), essContext.LookupStateProvinceByCode(cmd.Evacuee.MailingAddress.StateProvince));
            essContext.SetLink(contact, nameof(contact.era_MailingCity), essContext.LookupJurisdictionByCode(cmd.Evacuee.MailingAddress.Community));

            var results = await essContext.SaveChangesAsync();

            essContext.DetachAll();

            return new ManageEvacueeCommandResult { EvacueeId = contact.contactid.ToString() };
        }

        private contact GetContactIdForBcscId(string bcscId) =>
            string.IsNullOrEmpty(bcscId)
                ? null
                : essContext.contacts
                    .Where(c => c.era_bcservicescardid == bcscId)
                    .SingleOrDefault();

        private async Task<ManageEvacueeCommandResult> Handle(DeleteEvacuee cmd)
        {
            var contact = essContext.contacts.Where(c => c.contactid == Guid.Parse(cmd.Id)).SingleOrDefault();
            if (contact != null)
            {
                essContext.DeleteObject(contact);
                await essContext.SaveChangesAsync();
            }

            essContext.DetachAll();

            return new ManageEvacueeCommandResult { EvacueeId = cmd.Id };
        }

        private async Task<EvacueeQueryResult> Handle(EvacueeQuery query)
        {
            if (query.UserId == null && query.EvacueeId == null) throw new ArgumentNullException($"Must query registrants by user id or contact id");

            var readCtx = essContextFactory.CreateReadOnly();

            var contactQuery = readCtx.contacts
                  .Expand(c => c.era_City)
                  .Expand(c => c.era_ProvinceState)
                  .Expand(c => c.era_Country)
                  .Expand(c => c.era_MailingCity)
                  .Expand(c => c.era_MailingProvinceState)
                  .Expand(c => c.era_MailingCountry)
                  .Where(c => c.statecode == (int)EntityState.Active);

            if (!string.IsNullOrEmpty(query.EvacueeId)) contactQuery = contactQuery.Where(c => c.contactid == Guid.Parse(query.EvacueeId));
            if (!string.IsNullOrEmpty(query.UserId)) contactQuery = contactQuery.Where(c => c.era_bcservicescardid.Equals(query.UserId, StringComparison.OrdinalIgnoreCase));

            var contacts = await ((DataServiceQuery<contact>)contactQuery).GetAllPagesAsync();

            return new EvacueeQueryResult { Items = mapper.Map<IEnumerable<Evacuee>>(contacts, opt => opt.Items["MaskSecurityAnswers"] = query.MaskSecurityAnswers.ToString()) };
        }

        private async Task<ManageInvitationCommandResult> Handle(CreateNewEmailInvitation cmd)
        {
            var contact = await essContext.contacts.ByKey(Guid.Parse(cmd.EvacueeId)).GetValueAsync();
            var invitingTeamMember = string.IsNullOrEmpty(cmd.RequestingUserId)
                ? null
                : essContext.era_essteamusers.Where(m => m.statecode == (int)EntityState.Active && m.era_essteamuserid == Guid.Parse(cmd.RequestingUserId)).Single();

            //deactivate all current invites for this contact
            var currentInvites = await ((DataServiceQuery<era_evacueeemailinvite>)essContext.era_evacueeemailinvites
                .Where(i => i.statecode == (int)EntityState.Active && i._era_registrant_value == Guid.Parse(cmd.EvacueeId)))
                .GetAllPagesAsync();

            foreach (var currentInvite in currentInvites)
            {
                essContext.DeactivateObject(currentInvite, (int)EmailInviteStatus.Expired);
            }

            //create new invite
            var newInvite = new era_evacueeemailinvite
            {
                era_evacueeemailinviteid = Guid.NewGuid(),
                era_emailaddress = cmd.Email
            };

            essContext.AddToera_evacueeemailinvites(newInvite);

            //link to registrant and inviting user
            essContext.SetLink(newInvite, nameof(era_evacueeemailinvite.era_Registrant), contact);
            if (invitingTeamMember != null) essContext.SetLink(newInvite, nameof(era_evacueeemailinvite.era_ESSTeamUser), invitingTeamMember);

            await essContext.SaveChangesAsync();

            essContext.DetachAll();

            return new ManageInvitationCommandResult { InviteId = newInvite.era_evacueeemailinviteid.ToString() };
        }

        private async Task<InvitationQueryResult> Handle(EmailInvitationQuery query)
        {
            var invites = await ((DataServiceQuery<era_evacueeemailinvite>)essContext.era_evacueeemailinvites
                .Where(i => i.statecode == (int)EntityState.Active && i.era_evacueeemailinviteid == Guid.Parse(query.InviteId)))
                .GetAllPagesAsync();

            essContext.DetachAll();

            return new InvitationQueryResult
            {
                Items = mapper.Map<IEnumerable<Invitation>>(invites)
            };
        }

        private async Task<ManageInvitationCommandResult> Handle(CompleteInvitation cmd)
        {
            var invite = essContext.era_evacueeemailinvites
            .Where(i => i.statecode == (int)EntityState.Active && i.era_evacueeemailinviteid == Guid.Parse(cmd.InviteId))
            .SingleOrDefault();

            if (invite != null)
            {
                essContext.DeactivateObject(invite, (int)EmailInviteStatus.Used);
                await essContext.SaveChangesAsync();
            }

            return new ManageInvitationCommandResult { InviteId = cmd.InviteId };
        }
    }

    internal enum EmailInviteStatus
    {
        Active = 1,
        Used = 174360000,
        Expired = 2,
    }
}
