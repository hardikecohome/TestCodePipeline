using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IStorageServiceAgent
    {
        Task<Tuple<AgreementTemplateDTO, IList<Alert>>> UploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate);
    }
}
