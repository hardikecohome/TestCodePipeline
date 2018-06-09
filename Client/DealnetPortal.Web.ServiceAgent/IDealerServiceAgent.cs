﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;
    using DealnetPortal.Api.Models.Notify;

    /// <summary>
    /// Service agent for communicate with server-side service and controller for processing dealer's information
    /// </summary>
    public interface IDealerServiceAgent
    {
        Task<DealerProfileDTO> GetDealerProfile();
        Task<IList<Alert>> UpdateDealerProfile(DealerProfileDTO dealerProfile);
        Task<DealerInfoDTO> GetDealerOnboardingForm(string accessKey);
        Task<DealerInfoDTO> GetDealerOnboardingForm(int id);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> UpdateDealerOnboardingForm(DealerInfoDTO dealerInfo);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> SubmitDealerOnboardingForm(DealerInfoDTO dealerInfo);
        Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> AddDocumentToOnboardingForm(RequiredDocumentDTO document);
        Task<IList<Alert>> DeleteDocumentFromOnboardingForm(RequiredDocumentDTO document);
        Task<IList<Alert>> SendDealerOnboardingDraftLink(DraftLinkDTO link);
        Task<bool> CheckOnboardingLink(string dealerLink);
        Task<IList<Alert>> DealerSupportRequestEmail(SupportRequestDTO dealerSupportRequest);
    }
}
