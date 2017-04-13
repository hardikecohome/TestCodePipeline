using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IStorageServiceAgent
    {
        Task<Tuple<AgreementTemplateDTO, IList<Alert>>> UploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate);
    }
}
