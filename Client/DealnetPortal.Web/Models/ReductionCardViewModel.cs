using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Common.Enumeration;


namespace DealnetPortal.Web.Models
{
    public class ReductionCardViewModel
    {
        public string LoanAmortizationTerm { get; set; }

        public decimal CustomerReduction { get; set; }

        public decimal InterestRateReduction { get; set; }

    }
}