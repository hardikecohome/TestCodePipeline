using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class ApplicantsViewModel
    {
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public List<ApplicantPersonalInfo> AdditionalApplicants { get; set; }
        public int? ContractId { get; set; }
    }
}