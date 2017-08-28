using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.Dealer
{
    public class LeadsGenerationViewModel
    {
        public bool Referrals { get; set; }
        public bool LocalAds { get; set; }
        public bool TradeShows { get; set; }
    }
}