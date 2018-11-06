using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class BasicInfoViewModel
    {
        public string SubmittingDealerId { get; set; }
        [CheckHomeOwner("AdditionalApplicants")]
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public List<ApplicantPersonalInfo> AdditionalApplicants { get; set; }
        public List<SubDealer> SubDealers { get; set; }
        public int? ContractId { get; set; }
        public ContractState ContractState { get; set; }
        public bool ContractWasDeclined { get; set; }
        public bool QuebecDealer { get; set; }
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public IList<VarificationIdsDTO> VarificationIds { get; set; }
    }
}