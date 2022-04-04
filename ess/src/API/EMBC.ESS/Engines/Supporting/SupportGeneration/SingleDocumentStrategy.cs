﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EMBC.ESS.Engines.Supporting.SupportGeneration.ReferralPrinting;
using EMBC.ESS.Resources.Evacuees;
using EMBC.ESS.Resources.Metadata;
using EMBC.Utilities.Extensions;

namespace EMBC.ESS.Engines.Supporting.SupportGeneration
{
    internal class SingleDocumentStrategy : ISupportGenerationStrategy
    {
        private readonly IMapper mapper;
        private readonly IMetadataRepository metadataRepository;
        private readonly IEvacueesRepository evacueesRepository;

        public SingleDocumentStrategy(IMapper mapper, IMetadataRepository metadataRepository, IEvacueesRepository evacueesRepository)
        {
            this.mapper = mapper;
            this.metadataRepository = metadataRepository;
            this.evacueesRepository = evacueesRepository;
        }

        public async Task<GenerateResponse> Generate(GenerateRequest request)
        {
            if (!(request is GenerateReferralsRequest r))
                throw new InvalidOperationException($"{nameof(ISupportProcessingStrategy)} of type {nameof(SingleDocumentStrategy)} can only handle {nameof(GenerateReferralsRequest)} request types");

            return await GenerateSingleReferralDocument(r);
        }

        private async Task<GenerateReferralsResponse> GenerateSingleReferralDocument(GenerateReferralsRequest request)
        {
            var referrals = mapper.Map<IEnumerable<PrintReferral>>(request.Supports.Where(s => s.SupportDelivery is Shared.Contracts.Events.Referral), opts => opts.Items.Add("evacuationFile", request.File)).ToArray();
            var summaryItems = mapper.Map<IEnumerable<PrintSummary>>(request.Supports, opts => opts.Items.Add("evacuationFile", request.File)).ToArray();
            var printingUser = new PrintRequestingUser { Id = request.PrintingMember.Id, FirstName = request.PrintingMember.FirstName, LastName = request.PrintingMember.LastName };

            var communities = (await metadataRepository.GetCommunities()).ToDictionary(c => c.Code, c => c.Name);

            var recipientId = summaryItems.FirstOrDefault(s => s.IsEtransfer).NotificationInformation.RecipientId;
            var recipient = !string.IsNullOrEmpty(recipientId) ?
                (await evacueesRepository.Query(new EvacueeQuery { EvacueeId = summaryItems.FirstOrDefault(s => s.IsEtransfer).NotificationInformation.RecipientId })).Items.SingleOrDefault()
                : null;

            if (recipient != null)
            {
                foreach (var summary in summaryItems.Where(s => s.IsEtransfer))
                {
                    summary.NotificationInformation.RecipientName = recipient.FirstName + ' ' + recipient.LastName;
                }
            }

            foreach (var referral in referrals)
            {
                referral.VolunteerFirstName = printingUser.FirstName;
                referral.VolunteerLastName = printingUser.LastName;
                referral.DisplayWatermark = request.AddWatermark;
                referral.HostCommunity = communities.GetValueOrDefault(referral.HostCommunity);
                if (!string.IsNullOrEmpty(referral.Supplier?.Community)) referral.Supplier.City = communities.GetValueOrDefault(referral.Supplier.Community);
            }

            var title = $"supports-{request.File.Id}-{DateTime.UtcNow.ToPST().ToString("yyyyMMddhhmmss")}";
            var referralsHtml = ReferralHtmlGenerator.CreateSingleHtmlDocument(printingUser, referrals, summaryItems, request.AddSummary, request.AddWatermark, title);

            return new GenerateReferralsResponse
            {
                Content = Encoding.UTF8.GetBytes(referralsHtml)
            };
        }
    }
}
